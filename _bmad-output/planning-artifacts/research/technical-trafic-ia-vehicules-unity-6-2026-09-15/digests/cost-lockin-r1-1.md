# Digest — D5 Cost, lock-in, and the honest build estimate (round 1)

Research run 2026-09-15. Budget: 15 tool calls, 7 sources actually read.
**Headline: this evidence base is THIN, and the thinness is itself the finding.** See "Evidence quality note".

---

## Claims

**Claim 1 — A well-reviewed, still-listed commercial Unity traffic asset can be 3.5 years stale and still be sold at full price.** iTS – Intelligent Traffic System is at version 2.1.1, last updated **3 January 2023**, minimum Unity **2019.4.6**, sold at **€69** (Single Entity), publisher Jose Garrido. The listing carries no deprecation notice.
— https://assetstore.unity.com/packages/templates/systems/its-intelligent-traffic-system-23564 | Unity Asset Store | last update 2023-01-03 | accessed 2026-09-15 | confidence: **high** (read from the live store page today) | class: abandonment-risk, cost

**Claim 2 — iTS's critical reviews are about the authoring tool, not the driving.** Reviewer `syazmediaplt` (Not Recommended, ~6 years ago, v2.0): *"This is a very complex system and as a first timer I think it is really hard."* — citing no undo, a UI compared unfavourably to outdated software, thin tutorials, and **accidentally deleting entire lanes while editing points**. Separate reports: vehicles not instantiating in WebGL builds (v1.1.5), and inability to reach the iTS manager tools when adding the asset to an existing project (v1.1.5).
— https://assetstore.unity.com/packages/templates/systems/its-intelligent-traffic-system-23564/reviews | Unity Asset Store | reviews 6–7 years old, v1.1.5/v2.0 | accessed 2026-09-15 | confidence: **medium** (complaints predate v2.1.1; the "no undo / lane deletion" class of complaint is *not* verifiably fixed, but also not verifiably present today) | class: retrospective
> **Freshness caveat applied per brief:** these are 2019–2020 complaints about an asset whose last update is 2023. I could not verify whether the editor-UX complaints were fixed. Do not cite them as current defects.

**Claim 3 — A €321 traffic pack was reported broken on a Unity version two major releases behind current, six months ago.** Reviewer `dennismail4` on Urban Traffic System Full Pack (v2.0, ~6 months before 2026-09-15, i.e. ≈March 2026): *"The code still does not work in Unity 2022."* The review carries **4 helpful votes**, indicating the complaint is shared. Listed price **€321.10** (≈$350 USD full price).
— https://assetstore.unity.com/packages/templates/systems/urban-traffic-system-full-pack-166688/reviews | Unity Asset Store | review ≈2026-03 | accessed 2026-09-15 | confidence: **high** (recent, versioned, corroborated by votes) | class: retrospective, abandonment-risk, cost
> This is the single strongest datum in the digest: a **premium-priced** asset, a **recent** review, against the **then-current** version, failing on a Unity version already superseded by Unity 6.

**Claim 4 — At that price point, buyers still report doing integration work the asset did not do.** Same review page: `Mastashion` (~6 months ago, v2.1) reports demo scenes collapsing, undocumented layer settings, and missing car physics settings in the "Vehicles and Terrain" scene, and asks for documentation identifying *project-breaking* changes. `WebbyBoyStudios` states that at €321.10 the pack ought to include **pooling**. A further reviewer asked for guidance on integrating their own vehicle and character models.
— same URL as Claim 3 | accessed 2026-09-15 | confidence: **medium-high** | class: retrospective, cost
> Pattern, not proof: the reported friction is **integration and authoring**, never "the cars drive badly".

**Claim 5 — An actively maintained alternative exists and is cheaper than the premium pack.** Gley's Mobile Traffic System v3 is at **v3.6.4, released 4 September 2026** (11 days before access), **€118.69**, built on Unity 2022.3.62, supporting Built-in / URP / HDRP.
— https://assetstore.unity.com/packages/tools/behavior-ai/mobile-traffic-system-v3-305800 | Unity Asset Store | released 2026-09-04 | accessed 2026-09-15 | confidence: **high** | class: cost
> **Caveat I could not resolve:** Gley lists *Mobile Traffic System v2.0* (package 277301) and *v3* (package 305800) as **separate store listings**. I could not confirm from the pages whether v2 owners upgrade free or must repurchase. If the solo dev goes this route, **verify the v2→v3 upgrade policy before buying** — a paid major-version fork is a real lock-in cost and this is the exact shape it takes.

**Claim 6 — "I'll open-source mine" is a documented failure mode, and the stated reason is extraction cost, not laziness.** Unity Discussions thread (opened 27 Feb 2016, active to Jan 2018): `Marcos-Elias` announced a free open-source AI traffic system, motivated by *"All the best methods for placing AI cars into your projects were paid"* and *"Some cheap code I have tried was difficult to work with."* By Aug 2017: *"Currently I don't have enough time to make and release this for free… I still plan launching this some day, but unfortunately I cannot say 'when'."* Reason given: the system *"still depends on some parts of my other exclusive scripts"* and *"Separating the traffic system and writing documentation/tutorials would require a huge amount of hours."* It was never released. Another participant: *"2 years later… I'm making my own AI system for Unity now!"*
— https://discussions.unity.com/t/open-source-ai-traffic-system-that-really-works-out-of-the-box/618378 | Unity Discussions | 2016–2018 | accessed 2026-09-15 | confidence: **medium** (direct quotes, but 8–10 years old) | class: abandonment-risk, build-estimate
> Two things this evidences. (a) Traffic AI **entangles with the rest of the project** — that is the author's own diagnosis of why he could not cut it out. Expect the inverse on adoption: an asset built inside someone else's project shape will resist yours. (b) The free-thing-that-never-ships risk is documented, not hypothetical.

**Claim 7 — The community's own answer to "how do I build traffic?" contains no build-cost data.** Unity Discussions thread "Creating traffic system" (21–22 July 2024): the single responder recommends a spline-based approach on Unity's built-in Spline component, notes cars in games are *"mostly faked… they don't use actual physics to get around (until there's a crash, since you turn them on then)"*, and adds *"If you're not interested in coding your own, there's a bunch of systems available on the asset store that do this."* **No duration, no sub-problem ranking, no retrospective.**
— https://discussions.unity.com/t/creating-traffic-system/1487964 | Unity Discussions | 2024-07-21 | accessed 2026-09-15 | confidence: **high** (as a statement about what the thread contains) | class: build-estimate (negative result)

**Claim 8 — Absence of evidence, stated as a finding: I found no practitioner account giving a real duration for a from-scratch Unity city-traffic build.** Not one "took me N months" from a named developer, in seven sources read across Unity Discussions, the Asset Store, and general web search.
— confidence: **high** for the claim "I did not find it in this budget"; **low** for any inference that such accounts do not exist | class: build-estimate

---

## Retrospective accounts table

| Who | What they adopted or built | Timeframe reported | What they said months in | Source |
|---|---|---|---|---|
| `dennismail4` | Urban Traffic System Full Pack (€321.10), v2.0 | review ≈2026-03, i.e. against then-current version | *"The code still does not work in Unity 2022."* 4 helpful votes. | [reviews](https://assetstore.unity.com/packages/templates/systems/urban-traffic-system-full-pack-166688/reviews) |
| `Mastashion` | Urban Traffic System Full Pack, v2.1 | ≈2026-03 | Demo scenes collapse; layer settings undocumented; car physics settings missing from a demo scene; wants project-breaking changes flagged. | same |
| `WebbyBoyStudios` | Urban Traffic System Full Pack | ≈2026-03 | Works *after fixes*; argues pooling should be included at that price. | same |
| `syazmediaplt` | iTS – Intelligent Traffic System, v2.0 | ~6 yrs ago (stale) | Not Recommended. No undo; editing points can delete whole lanes; tutorials inadequate. | [reviews](https://assetstore.unity.com/packages/templates/systems/its-intelligent-traffic-system-23564/reviews) |
| `prakashpv` | iTS v1.1.5 | ~7 yrs ago (stale) | Cars not instantiating in WebGL build; fix was to disable multi-threading on the Car Spawner. | same |
| `Marcos-Elias` | Built his own, intended to open-source it | 2016 → abandoned by 2018 | Could not extract it from his project: *"still depends on some parts of my other exclusive scripts"*; extraction + docs = *"a huge amount of hours."* | [thread](https://discussions.unity.com/t/open-source-ai-traffic-system-that-really-works-out-of-the-box/618378) |
| anon. participant | Same thread | *"2 years later… I'm making my own AI system for Unity now!"* | Chose build-own after surveying paid options. | same |

**The table is short and skews to Asset Store reviews. That is an accurate picture of what is publicly available, not a sampling shortcut.**

---

## The time sink: which sub-problem does the evidence actually name?

The brief offered three hypotheses: lane/road **authoring tooling**, **intersection deadlock**, **blocked-vehicle recovery**.

**What the evidence supports: authoring tooling and integration — not the driving logic, and not (in this evidence) deadlock.**

- Every user-side complaint I read that names a *specific* pain names the editor/authoring layer or project integration: no undo and accidental lane deletion (iTS), undocumented layer setup and broken demo scenes (Urban Traffic System), inability to reach the manager tools when dropping the asset into an existing project (iTS), integrating your own vehicle models (Urban Traffic System).
- **Zero** reviews or forum posts I read complained that vehicles deadlock at intersections or that stuck-car recovery was missing. That is a striking null.
- Build-side, `Marcos-Elias`'s diagnosis points the same direction: the expensive part was not the driving model, it was **separating the system from its host project and documenting it** — i.e. the authoring/integration surface again.

**Honest qualifier, and it matters.** This is a real skew in the *sample*, not necessarily in *reality*. Asset Store reviews are written by people in week 1–4 of adoption, when authoring UX is exactly what you meet first; intersection deadlock and blocked-vehicle recovery are month-3 problems that show up in a shipped build, and the people who hit them post in Discord or nowhere. **Treat "authoring is the time sink" as the evidenced hypothesis, with the deadlock hypothesis untested rather than refuted.** I could not reach practitioner accounts at the depth that would settle it (see below).

A search snippet attributed to the CBLab paper (arXiv 2210.00896) states that in simulators like SUMO and CityFlow deadlocks occur very frequently in practice and packages use ad hoc heuristic rules to handle them. **I did not read that paper this run and am not citing it as a finding** — it is listed under Leads.

---

## Evidence quality note (plainly)

**Thin, and lopsided.** Seven sources read. The quality distribution:

- **Strong and fresh:** current store facts (version, date, price) for three named assets, read directly off the live pages today. These meet the ≤3-month price/licence bar.
- **Strong but narrow:** ~2026-03 critical reviews on Urban Traffic System Full Pack — recent, version-pinned, vote-corroborated. This is the digest's best retrospective evidence, and it is three reviews.
- **Weak:** iTS reviews are 6–7 years old against an asset last touched in 2023. Flagged, not counted as current defects.
- **Aged:** the abandonment thread is 2016–2018. Directionally useful, outside the ≤2-year pattern bar.
- **Missing entirely:** from-scratch build durations; month-six adoption retrospectives; any devlog or postmortem with numbers.

**Two structural limits on this run, both worth recording:**

1. **Reddit was inaccessible.** `reddit.com` is blocked to this user agent (the search tool refused the domain outright). r/Unity3D and r/gamedev were named in the brief as primary hunting grounds and **could not be searched at all**. A meaningful share of the evidence this dimension wants probably lives there and is simply unreachable from this harness.
2. **A comparative practitioner review was paywalled/blocked.** The Medium article "Unity Car & Vehicle Traffic AI Systems Performance Reviews" (spakment.medium.com) returned HTTP 403. Search snippets attribute to it: performance ranging 8 ms/frame for ~100 cars in complex environments down to 0.8 ms for simpler setups; criticism that some systems are *"just loops"* with no real logic and no pooling; and the observation that source-code availability matters because *"no generic system fits 100% any project."* **Those figures are unverified — do not carry them into a decision without reading the article.**

**Two-source test result:** "adopting a traffic asset costs real integration work" clears it (Urban Traffic System reviews + the Marcos-Elias extraction account, different directions, same conclusion). **"Building from scratch takes N months" fails it — there is no first source, let alone two.**

---

## Leads worth chasing

1. **Read the Medium comparison via another route** (archive.org, Google cache, a non-Medium mirror). It is the only comparative practitioner benchmark surfaced, and its performance numbers would materially inform the decision.
2. **Reddit, from a harness that can reach it.** r/Unity3D + r/gamedev search for `traffic system` sorted by top/year. This is the highest-expected-value unexplored seam.
3. **Confirm Gley's v2→v3 upgrade policy** at gleygames.com or by asking the publisher. Direct lock-in cost, resolvable with one email.
4. **The asset's Discord/support forum, not its store page.** iTS points at `forums.dagagames.com`; Gley at `gleygames.com`. Month-six problems get reported to support, not to reviews. Check whether those channels are *alive* — a dead support forum on a €69 asset is an abandonment signal the store page will never show.
5. **CBLab (arXiv 2210.00896)** on deadlock frequency in SUMO/CityFlow — read it directly before using it; it is academic traffic simulation, not game AI, so its transfer to a game context needs an argument.
6. **YouTube devlogs** were never searched (budget). Devlog series are where durations actually get stated.

---

## What I looked for and could NOT find

- **Any named developer stating how long a from-scratch Unity city-traffic system took them.** Zero. This is the single biggest hole, and it is the question the solo dev most needs answered.
- **Any month-six adoption retrospective** — "I bought X, here is what happened after shipping". Asset Store reviews are week-one artefacts; nothing longer-horizon surfaced.
- **Any account of hitting an extension wall on a *named* modern traffic asset** — monolithic code, no extension points, DLL-only, or updates overwriting local edits. The brief asked me to name names; **the public evidence did not let me.** (`Marcos-Elias`'s *"some cheap code I have tried was difficult to work with"* is unnamed and 10 years old.)
- **The inverse case** — a documented account of an open, well-structured traffic system being genuinely easy to extend. Not found.
- **Any evidence at all on the "reference implementation" middle path** — reading an open-source system and reimplementing its ideas rather than depending on it. **I found nothing. Not thin: empty.** The brief predicted this option would be under-evidenced; that prediction is confirmed. No practitioner account, no comparison, nothing. Anyone choosing that path for this project is choosing it on reasoning, not on precedent.
- **Documented precedent for "free asset later went paid" or "asset delisted"** in the traffic-asset space specifically. Search returned only generic itch.io chatter about assets being removed; nothing solid enough to cite. The closest *structural* signal is Gley shipping v2 and v3 as separate listings (Claim 5) — suggestive of paid major-version forks, **unconfirmed**.
- **Asset abandonment *rates*** — any study or dataset on what fraction of Unity assets go stale. Does not appear to exist publicly.
