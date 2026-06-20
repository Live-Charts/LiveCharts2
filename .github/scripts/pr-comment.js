// Shared helper for the PR Comment workflow.
//
// The PR Comment workflow runs two jobs (one per upstream workflow). Both jobs
// share a single PR comment; each job edits only its own section so partial
// reruns and out-of-order completions never clobber the other section.
//
// Section content lives between marker lines (e.g. `<!-- section:livecharts:start -->`
// ... `<!-- section:livecharts:end -->`). When a section is missing, upsertSection
// appends it; when present, it replaces in place.

const TAG = (prNumber) => `<!-- livecharts-bot:pr-${prNumber} -->`;
const LEGACY_PERF_TAG = (prNumber) => `<!-- livecharts-bot:perf-pr-${prNumber} -->`;

const SECTIONS = {
  livecharts: {
    start: '<!-- section:livecharts:start -->',
    end:   '<!-- section:livecharts:end -->',
  },
  benchmarks: {
    start: '<!-- section:benchmarks:start -->',
    end:   '<!-- section:benchmarks:end -->',
  },
};

// Platform groupings rendered inside the Tests <details>. Each entry must
// match exactly one matrix variant from the LiveCharts workflow:
//   - testId / jobName / tf -> the artifact name
//     `test-results-{testId}-{jobName}-{tf}-attempt-{runAttempt}` (the
//     run_attempt suffix is appended by buildTestsBlock at lookup time).
//   - matrix -> values that must prefix-match the parenthesised matrix part of
//     the corresponding job's display name (declaration order, id-first).
const PLATFORMS = [
  { name: 'Avalonia', entries: [
    { label: 'desktop (windows)', testId: 'avalonia-desktop', jobName: 'test-windows', tf: '',                                 matrix: ['avalonia-desktop'] },
    { label: 'desktop (linux)',   testId: 'avalonia-desktop', jobName: 'test-linux',   tf: '',                                 matrix: ['avalonia-desktop'] },
    { label: 'desktop (mac)',     testId: 'avalonia-desktop', jobName: 'test-mac',     tf: '',                                 matrix: ['avalonia-desktop'] },
    { label: 'android',           testId: 'avalonia-android', jobName: 'test-android', tf: '',                                 matrix: ['avalonia-android'] },
    { label: 'ios',               testId: 'avalonia-ios',     jobName: 'test-ios',     tf: '',                                 matrix: ['avalonia-ios'] },
    { label: 'browser',           testId: 'avalonia-browser', jobName: 'test-browser', tf: '',                                 matrix: ['avalonia-browser'] },
  ]},
  { name: 'Uno', entries: [
    { label: 'desktop (windows)',         testId: 'uno', jobName: 'test-windows', tf: 'net10.0-desktop',                matrix: ['uno', 'net10.0-desktop'] },
    { label: 'win10.0.19041 (windows)',   testId: 'uno', jobName: 'test-windows', tf: 'net10.0-windows10.0.19041.0',     matrix: ['uno', 'net10.0-windows10.0.19041.0'] },
    { label: 'desktop (linux)',           testId: 'uno', jobName: 'test-linux',   tf: 'net10.0-desktop',                matrix: ['uno', 'net10.0-desktop'] },
    { label: 'desktop (mac)',             testId: 'uno', jobName: 'test-mac',     tf: 'net10.0-desktop',                matrix: ['uno', 'net10.0-desktop'] },
  ]},
  { name: 'MAUI', entries: [
    { label: 'windows',     testId: 'maui',     jobName: 'test-windows', tf: 'net10.0-windows10.0.19041.0', matrix: ['maui', 'net10.0-windows10.0.19041.0'] },
    { label: 'maccatalyst', testId: 'maui',     jobName: 'test-mac',     tf: 'net10.0-maccatalyst',         matrix: ['maui', 'net10.0-maccatalyst'] },
    { label: 'android',     testId: 'maui',     jobName: 'test-android', tf: 'net10.0-android',             matrix: ['maui', 'net10.0-android'] },
    { label: 'ios',         testId: 'maui-ios', jobName: 'test-ios',     tf: 'net10.0-ios',                 matrix: ['maui-ios', 'net10.0-ios'] },
  ]},
  { name: 'WinUI', entries: [
    { label: 'windows', testId: 'winui', jobName: 'test-windows', tf: '', matrix: ['winui'] },
  ]},
  { name: 'WinForms', entries: [
    { label: 'net10 (windows)',         testId: 'winforms-net10',       jobName: 'test-windows', tf: '', matrix: ['winforms-net10'] },
    { label: 'net10-19041 (windows)',   testId: 'winforms-net10w19041', jobName: 'test-windows', tf: '', matrix: ['winforms-net10w19041'] },
    { label: 'net462 (windows)',        testId: 'winforms-net462',      jobName: 'test-windows', tf: '', matrix: ['winforms-net462'] },
  ]},
  { name: 'WPF', entries: [
    { label: 'net10 (windows)',         testId: 'wpf-net10',         jobName: 'test-windows', tf: '', matrix: ['wpf-net10'] },
    { label: 'net10-19041 (windows)',   testId: 'wpf-net10w19041',   jobName: 'test-windows', tf: '', matrix: ['wpf-net10w19041'] },
    { label: 'net462 (windows)',        testId: 'wpf-net462',        jobName: 'test-windows', tf: '', matrix: ['wpf-net462'] },
  ]},
  { name: 'Eto', entries: [
    { label: 'windows', testId: 'eto', jobName: 'test-windows', tf: '', matrix: ['eto'] },
    { label: 'mac',     testId: 'eto', jobName: 'test-mac',     tf: '', matrix: ['eto'] },
  ]},
  { name: 'Core', entries: [
    { label: 'net8.0', testId: 'core-net8.0', jobName: 'test-core', tf: 'net8.0', matrix: ['net8.0'] },
    { label: 'net462', testId: 'core-net462', jobName: 'test-core', tf: 'net462', matrix: ['net462'] },
  ]},
  { name: 'Snapshot', entries: [
    { label: 'snapshot', testId: 'snapshot', jobName: 'test-snapshot', tf: '', matrix: [] },
  ]},
];

const STATUS_EMOJI = {
  success: '✅', failure: '❌', cancelled: '⚠️',
  skipped: '⏭️', timed_out: '❌', missing: '❔', unknown: '❔',
};

function emoji(s) { return STATUS_EMOJI[s] || '❔'; }

function aggregate(statuses) {
  if (statuses.length === 0) return 'missing';
  if (statuses.some(s => s === 'failure' || s === 'timed_out')) return 'failure';
  if (statuses.some(s => s === 'cancelled')) return 'cancelled';
  if (statuses.every(s => s === 'success')) return 'success';
  if (statuses.some(s => s === 'skipped')) return 'skipped';
  return 'unknown';
}

function statusOf(job) {
  if (!job) return 'missing';
  if (job.status !== 'completed') return 'unknown';
  return job.conclusion || 'unknown';
}

// Find the matrix variant of `baseJob` whose parenthesised values prefix-match
// `requiredValues`. Required values must be in YAML declaration order (id, tf).
// Workloads (when present in the matrix) appears last and is intentionally not
// part of the required values, but the prefix match still disambiguates because
// `id` is always first.
function findMatrixJob(jobs, baseJob, requiredValues) {
  if (requiredValues.length === 0) {
    return jobs.find(j => j.name === baseJob);
  }
  const prefix = baseJob + ' (';
  return jobs.find(j => {
    if (!j.name.startsWith(prefix) || !j.name.endsWith(')')) return false;
    const inner = j.name.slice(prefix.length, -1);
    const parts = inner.split(', ');
    if (parts.length < requiredValues.length) return false;
    return requiredValues.every((v, i) => parts[i] === v);
  });
}

function aggregateBaseJob(jobs, baseJob) {
  const matching = jobs.filter(j => j.name === baseJob || j.name.startsWith(baseJob + ' ('));
  return aggregate(matching.map(statusOf));
}

function artifactUrl(owner, repo, runId, artifact) {
  return `https://github.com/${owner}/${repo}/actions/runs/${runId}/artifacts/${artifact.id}`;
}

function buildTestsBlock({ jobs, artifacts, runId, runAttempt, owner, repo }) {
  const groups = [];
  const overall = [];
  for (const platform of PLATFORMS) {
    const lines = [];
    const groupStatuses = [];
    for (const e of platform.entries) {
      const job = findMatrixJob(jobs, e.jobName, e.matrix);
      const status = statusOf(job);
      if (status === 'missing') continue; // matrix entry was not included in this run
      groupStatuses.push(status);
      const artifactName = `test-results-${e.testId}-${e.jobName}-${e.tf}-attempt-${runAttempt}`;
      const artifact = artifacts.find(a => a.name === artifactName && !a.expired);
      const link = artifact ? `[trx](${artifactUrl(owner, repo, runId, artifact)})` : '_no trx_';
      lines.push(`  - ${e.label} ${emoji(status)} — ${link}`);
    }
    if (groupStatuses.length === 0) continue;
    const groupStatus = aggregate(groupStatuses);
    overall.push(groupStatus);
    groups.push({ name: platform.name, status: groupStatus, lines });
  }

  const summary = aggregate(overall);
  const inner = groups
    .map(g => `- **${g.name}** ${emoji(g.status)}\n${g.lines.join('\n')}`)
    .join('\n');

  return [
    `#### Tests ${emoji(summary)}`,
    ``,
    `<details><summary>Show details</summary>`,
    ``,
    inner,
    ``,
    `</details>`,
  ].join('\n');
}

// Builds the Code Coverage block: an H4 header that announces direction (up /
// down / unchanged) and the delta, then a collapsed <details> with the
// before/after table, a link to the full report, and a per-class diff table
// (when a baseline Summary.json was available on the badges branch).
function buildCoverageBlock({ summary, oldClasses, newClasses, reportUrl }) {
  let phrase;
  if (summary.delta === 'N/A') {
    phrase = `no baseline (current: ${summary.new})`;
  } else if (summary.delta === '0%') {
    phrase = `unchanged at ${summary.new}`;
  } else if (summary.delta.startsWith('+')) {
    phrase = `increased ${summary.delta} (${summary.old} → ${summary.new})`;
  } else {
    phrase = `decreased ${summary.delta} (${summary.old} → ${summary.new})`;
  }

  const flatten = (s) => {
    const out = new Map();
    const assemblies = s?.coverage?.assemblies || [];
    for (const a of assemblies) {
      for (const c of (a.classesinassembly || [])) {
        out.set(`${a.name} / ${c.name}`, c.coverage);
      }
    }
    return out;
  };

  const oldMap = flatten(oldClasses);
  const newMap = flatten(newClasses);

  let classesTable = '';
  if (oldMap.size > 0 || newMap.size > 0) {
    const allKeys = new Set([...oldMap.keys(), ...newMap.keys()]);
    const changes = [];
    for (const k of allKeys) {
      const o = oldMap.get(k);
      const n = newMap.get(k);
      if (o === n) continue;
      const delta = (o !== undefined && n !== undefined) ? (n - o) : null;
      changes.push({ name: k, old: o, new: n, delta });
    }
    // Sort by absolute delta (added/removed classes — null delta — float to top).
    changes.sort((a, b) => {
      const av = a.delta === null ? Infinity : Math.abs(a.delta);
      const bv = b.delta === null ? Infinity : Math.abs(b.delta);
      return bv - av;
    });

    const fmt = (v) => v === undefined ? '_n/a_' : `${v.toFixed(1)}%`;
    const fmtDelta = (d) => {
      if (d === null) return '—';
      if (d > 0) return `+${d.toFixed(1)}%`;
      return `${d.toFixed(1)}%`;
    };

    if (oldMap.size === 0) {
      classesTable = '\n\n_No baseline class data on the badges branch yet — per-class diff will populate on the next PR after this lands._';
    } else if (changes.length === 0) {
      classesTable = '\n\n_No per-class coverage changes._';
    } else {
      const cap = 50;
      const top = changes.slice(0, cap);
      classesTable =
        '\n\n**Class coverage changes**\n\n' +
        '| Class | Before | After | Delta |\n|---|---|---|---|\n' +
        top.map(c => `| ${c.name} | ${fmt(c.old)} | ${fmt(c.new)} | ${fmtDelta(c.delta)} |`).join('\n');
      if (changes.length > cap) {
        classesTable += `\n\n_…and ${changes.length - cap} more not shown._`;
      }
    }
  }

  return [
    `#### Code Coverage ${summary.emoji} ${phrase}`,
    ``,
    `<details><summary>Show details</summary>`,
    ``,
    `| | Line Coverage |`,
    `|---|---|`,
    `| **Before** | ${summary.old} |`,
    `| **After** | ${summary.new} |`,
    `| **Delta** | ${summary.delta} |`,
    ``,
    `Full coverage report: ${reportUrl}`,
    classesTable,
    ``,
    `</details>`,
  ].join('\n');
}

function upsertSection(body, section, content) {
  const { start, end } = SECTIONS[section];
  const startIdx = body.indexOf(start);
  const endIdx = body.indexOf(end);
  if (startIdx !== -1 && endIdx !== -1 && endIdx > startIdx) {
    return body.slice(0, startIdx + start.length) + '\n' + content + '\n' + body.slice(endIdx);
  }
  return body.replace(/\s*$/, '') + '\n\n' + start + '\n' + content + '\n' + end;
}

function buildSkeleton(prNumber, section, content) {
  return [
    TAG(prNumber),
    `#### Thanks for your contribution!`,
    ``,
    SECTIONS[section].start,
    content,
    SECTIONS[section].end,
  ].join('\n');
}

async function resolvePr({ github, context, headOwner, headBranch }) {
  const owner = context.repo.owner;
  const repo = context.repo.repo;
  const wrPrs = context.payload.workflow_run.pull_requests || [];
  let prNumber = wrPrs.find(p => p.number)?.number;
  let baseRef = null;
  if (!prNumber) {
    const { data: prs } = await github.rest.pulls.list({
      owner, repo, state: 'open',
      head: `${headOwner}:${headBranch}`,
    });
    prNumber = prs[0]?.number;
    baseRef = prs[0]?.base?.ref;
  }
  return { prNumber, baseRef };
}

async function upsertComment({ github, context, prNumber, section, content }) {
  const owner = context.repo.owner;
  const repo = context.repo.repo;
  const tag = TAG(prNumber);
  const comments = await github.paginate(github.rest.issues.listComments, {
    owner, repo, issue_number: prNumber, per_page: 100,
  });
  const existing = comments.find(c => c.body && c.body.includes(tag));
  if (existing) {
    const body = upsertSection(existing.body, section, content);
    await github.rest.issues.updateComment({ owner, repo, comment_id: existing.id, body });
  } else {
    const body = buildSkeleton(prNumber, section, content);
    await github.rest.issues.createComment({ owner, repo, issue_number: prNumber, body });
  }

  // Best-effort cleanup of the previous standalone benchmark comment, now
  // folded into the merged comment.
  const legacy = LEGACY_PERF_TAG(prNumber);
  for (const c of comments) {
    if (c.body && c.body.includes(legacy)) {
      try {
        await github.rest.issues.deleteComment({ owner, repo, comment_id: c.id });
      } catch (e) {
        // ignore — best-effort
      }
    }
  }
}

module.exports = {
  TAG,
  SECTIONS,
  PLATFORMS,
  emoji,
  aggregate,
  aggregateBaseJob,
  statusOf,
  findMatrixJob,
  artifactUrl,
  buildTestsBlock,
  buildCoverageBlock,
  upsertSection,
  buildSkeleton,
  resolvePr,
  upsertComment,
};
