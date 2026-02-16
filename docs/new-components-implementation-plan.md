# Implementation Plan: Smart Translate, Smart Summary, Smart Image
**Author:** Senior Principal Software Engineer  
**Date:** 2026-02-16  
**Status:** Draft (Rev 2 — incorporates engineering review)

## Architectural Overview

We will leverage the existing `SmartComponents.Inference` and `SmartComponents.AspNetCore` architecture. These new components fit naturally into our pattern: **Client/Server Component → Inference Backend → LLM**.

### Core Design Principles
1.  **Backend-Driven Intelligence:** All AI logic resides in `SmartComponents.Inference`. The UI components are "dumb" renderers that request processed data.
2.  **Progressive Enhancement:** Components must work (or fail gracefully) if the AI service is unavailable. See fallback behavior defined in Product Scope.
3.  **Token Efficiency:** We must use targeted prompts to minimize token usage.
4.  **Flexible Execution:** All components must support `Manual` (button click) and `Automatic` (debounced/on-blur) execution modes.
5.  **Cancellation:** All in-flight requests must be cancellable via `CancellationTokenSource`. If a user triggers a new request, the previous one is cancelled automatically.
6.  **Rate Limiting:** Automatic mode enforces `MinIntervalMs` and `MaxRequestsPerMinute` guards to prevent API bill explosion.
7.  **Observability:** All inference calls emit `OnInferenceComplete` and `OnInferenceError` callbacks with metadata (tokens, latency, model).

### Common Component API
All components (`SmartTranslate`, `SmartSummary`, `SmartImage`) will expose standard parameters for execution control:
-   `ExecutionMode`: `Manual` | `Automatic` (Default: `Manual`)
-   `AutoTrigger`: `Debounce` | `OnBlur` (Default: `Debounce`)
-   `DebounceMs`: `int` (Default: `500` — relevant only if Trigger is Debounce)
-   `MinIntervalMs`: `int` (Default: `1000` — minimum time between requests)
-   `MaxRequestsPerMinute`: `int` (Default: `10` — hard cap)

### Shared Base Class: `SmartComponentBase`
Extract a `SmartComponentBase` Blazor base class that handles:
-   Execution mode switching (Manual / Automatic)
-   Debounce and OnBlur trigger logic
-   Rate limiting (`MinIntervalMs`, `MaxRequestsPerMinute`)
-   Cancellation token lifecycle
-   Loading / Error / Success state management
-   `OnInferenceComplete` / `OnInferenceError` callback dispatching

> **This is Phase 1 work** — it is foundational and must be built before any individual component.

---

## 1. Smart Translate Implementation *(P1)*

### Architecture
-   **Backend (`SmartComponents.Inference`):**
    -   New Interface: `ISmartTranslateInference`
    -   New Service: `SmartTranslateInference`
    -   Input:
        -   `originalText`
        -   `targetLanguage`
        -   `userInstructions` (optional, for tone/refinement, e.g., "Make it formal")
        -   `pageContext` (optional, for domain awareness)
        -   `glossary` (optional, `Dictionary<string, string>` — domain-specific terminology constraints)
        -   `previousTranslation` (optional, for refinement rounds)
    -   Output: `translatedText`
    -   The source language is **auto-detected** by the LLM — no explicit `sourceLanguage` parameter needed.
-   **Frontend (`SmartComponents.AspNetCore` / `Blazor`):**
    -   Component: `<SmartTranslate TargetId="myInput" ... />`
    -   **UX Pattern:** Unobtrusive Wrapper / Popover.
    -   **Hotkey:** Default `Ctrl+Shift+T`, overridable via `Hotkey` parameter.
    -   **Popover State:**
        -   `OriginalText`: The source text.
        -   `DraftText`: The text returned by the AI.
        -   `DiffView`: Visual diff (strikethrough old → highlighted new) shown by default.
        -   `UserFeedback`: Text input for refining instructions.
        -   `RefinementCount`: Tracks rounds (max 3).
    -   **Actions:**
        -   `Refine`: Calls backend again with `originalText` + `previousTranslation` + `UserFeedback`. Capped at 3 rounds — after that, shows "Start Fresh" button.
        -   `Replace`: Uses JS Interop to set the value of the target input to `DraftText`.
        -   *(V2)* `Insert at Cursor`: Deferred — DOM complexity across browsers (especially `contenteditable`) is high. See SmartMic precedent.

### Challenges & Mitigations
-   **DOM Interactions:** Replace-all is straightforward. Insert-at-cursor deferred to V2.
    -   *Mitigation:* Use a dedicated TypeScript module `SmartTranslate.ts` for JS Interop.
-   **Token Accumulation:** The "Refine" loop sends growing context each round.
    -   *Mitigation:* Hard cap at 3 refinement rounds. "Start Fresh" resets the conversation.
-   **Glossary Prompt Engineering:** Injecting glossary terms into the prompt without confusing the model.
    -   *Mitigation:* Use a structured system prompt section: `"You MUST use the following terminology: ..."`.

---

## 2. Smart Summary Implementation *(P2)*

### Architecture
-   **Backend (`SmartComponents.Inference`):**
    -   New Interface: `ISmartSummaryInference`
    -   New Service: `SmartSummaryInference`
    -   Input: `text`, `lengthPreference` (TlDr / KeyPoints / FullBrief), `focusArea` (optional).
    -   Output: Markdown string with **source citations** (paragraph/sentence indices from the original text).
    -   **Streaming:** The backend must support streaming the response token-by-token via `IAsyncEnumerable<string>`.
-   **Frontend:**
    -   Component: `<SmartSummary Text="@sourceText" />`
    -   Display Modes:
        -   **Collapsible Card (Default):** "Show Summary" → Expands to show the AI output.
        -   **Inline Tooltip / Popover:** Hover over truncated text to see a 1-sentence TL;DR. Ideal for list views and dashboards.
    -   **Source Linking UI:** Clicking a citation highlights the corresponding section in the source text (requires the source text to be rendered on-page or in a side panel).
    -   **Streaming UI:** Tokens appear incrementally in the summary area.

### Challenges & Mitigations
-   **Context Window:** Large texts (PDFs, logs) might exceed token limits.
    -   *Mitigation:* For V1, truncate input at token limit with a warning. Future: Implement Map-Reduce summarization.
-   **Source Linking Accuracy:** LLMs may hallucinate citation indices.
    -   *Mitigation:* Use a structured output format (JSON with `[{summary_sentence, source_range}]`) and validate indices against the original text length. Discard invalid citations silently.

---

## 3. Smart Image Implementation *(P3)*

### Architecture
-   **Backend (`SmartComponents.Inference`):**
    -   New Interface: `ISmartImageInference`
    -   New Service: `SmartImageInference`
    -   Input:
        -   `ImageUrl` | `Base64Stream`
        -   `ModelOverride` (string, optional): Allows specifying a dedicated vision model (e.g., `google/gemini-flash-1.5`).
        -   `EnableSafetyCheck` (bool, default: false)
        -   `SafetyThreshold` (Low | Medium | High, default: Medium)
    -   Task: Vision Capability (requires model with vision support).
    -   Output: JSON object containing:
        -   `AltText` (string)
        -   `IsSafe` (boolean)
        -   `FocalPoint` ( { x: float, y: float } ) — Normalized coordinates (0-1) for smart cropping.
-   **Frontend (`SmartComponents.AspNetCore` / `Blazor`):**
    -   Component: `<SmartImage Src="..." Model="openai/gpt-4o" ExecutionMode="Automatic" />`
    -   **Two-Tier Focal Point Detection:**
        -   **Tier 1 (Client-Side, Fast):** Use the browser's `FaceDetector` API (Chromium) or a lightweight WASM model (BlazeFace, ~2KB). If faces detected → use center of bounding box as focal point. Skip LLM call for focal point.
        -   **Tier 2 (LLM Fallback):** If Tier 1 returns no results → request focal point from the Vision model alongside alt-text (single LLM call for both).
    -   **Smart Crop Logic:** CSS utilization of `object-position` based on returned `FocalPoint` to ensure the subject stays visible.
    -   **Fallback:** If inference is unavailable, render a standard `<img>` tag with developer-supplied `FallbackAlt` text.

### Challenges & Mitigations
-   **Performance:** Vision calls are slow (~2-5s).
    -   *Mitigation:* Cache results heavily based on **Image Hash + ModelOverride** (both are part of the cache key).
-   **Cost:** Vision tokens are expensive.
    -   *Mitigation:* Resize large images to standard dimensions (e.g., max 512px on longest side, **preserving aspect ratio** via fit-within-bounds) before sending to LLM.
-   **SignalR Message Size:** Sending Base64 images over SignalR has a default 32KB hub message size limit.
    -   *Mitigation:* Document the required `MaximumReceiveMessageSize` configuration increase. For large images, consider a chunked upload approach or a separate HTTP endpoint.

---

## Proposed Roadmap

### Phase 1: Foundation + Core Inference (Weeks 1-2)
1.  **`SmartComponentBase`** — Shared Blazor base class (execution modes, debounce, rate limiting, cancellation, observability callbacks). **This is prerequisite for all components.**
2.  Define `ISmartTranslateInference`, `ISmartSummaryInference`, `ISmartImageInference`.
3.  Implement `SmartTranslateInference` backend with glossary and auto-detection support.
4.  Implement `SmartSummaryInference` backend with streaming (`IAsyncEnumerable`) and source citations.

### Phase 2: Frontend Components — Translate & Summary (Weeks 3-4)
1.  Build `<SmartTranslate>` popover UI, diff view, `SmartTranslate.ts` for JS Interop (replace only).
2.  Build `<SmartSummary>` collapsible card + inline tooltip modes with streaming UI and source linking.
3.  Wire Manual/Automatic execution via `SmartComponentBase`.

### Phase 3: Smart Image + Polish (Weeks 5-6)
1.  Implement `SmartImageInference` backend with `ModelOverride` and safety check.
2.  Build `<SmartImage>` Blazor component with two-tier focal point detection (client-side + LLM fallback).
3.  Implement `object-position` CSS logic and fallback rendering.
4.  Caching layer for image inference results.
5.  Streaming support for long translations (if not done in Phase 2).
6.  End-to-end testing across all components.
