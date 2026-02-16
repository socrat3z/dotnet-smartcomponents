# Product Scope: Smart Translate, Smart Summary, Smart Image
**Author:** Senior Product Owner  
**Date:** 2026-02-16  
**Status:** Draft (Rev 2 — incorporates engineering review)

## Executive Summary
We are expanding the "Smart Components" suite to capture three critical areas of modern web application friction: **Globalization**, **Information Overload**, and **Accessibility**.

By integrating **Smart Translate**, **Smart Summary**, and **Smart Image**, we move beyond simple form helpers (Smart Paste) into full-fledged content intelligence. These components allow developers to build apps that are globally ready instantly, capable of distilling large volumes of data for decision-makers, and accessible by default.

### Priority Order (MoSCoW)
| Priority | Component | Rationale |
|---|---|---|
| **P1 — Must Have** | Smart Translate | Closest to existing SmartPaste UX (popover + text). Highest enterprise demand. No vision model complexity. |
| **P2 — Must Have** | Smart Summary | Simple backend. High ROI for enterprise dashboards and support tools. |
| **P3 — Should Have** | Smart Image | Most complex (vision models, image pipelines, caching). Ship alt-text first, focal point second. |

---

## Common Core Features
**Applicable to all Smart Components**

### Execution Modes
To balance cost, performance, and user control, all components must support two execution flows:
1.  **Manual (Default):** AI processing is triggered only when the user explicitly clicks a button (e.g., "Summarize", "Translate", "Analyze Image").
2.  **Automatic:** AI processing happens in the background without user intervention.

### Automatic Trigger Strategies
When in **Automatic** mode, developers can choose *when* the AI runs:
-   **Throttling (Debounce):** Updates occur automatically after the user stops typing/interacting for a set time (e.g., 500ms). Perfect for "live" feedback.
-   **On Blur:** Updates occur only when the user leaves the input field or component. Perfect for validation or finalization.

### Rate Limiting
Automatic mode with fast user input can cause API bill explosion. Debounce alone is not sufficient:
-   All components enforce a `MinIntervalMs` floor between requests.
-   All components enforce a `MaxRequestsPerMinute` guard (default: 10).

### Cancellation
If the user triggers a new request while a previous one is in-flight, the previous request must be cancelled. Components use `CancellationTokenSource` internally and expose a `Cancel()` method.

### Observability
Enterprise customers budget AI spend. All components must expose:
-   `OnInferenceComplete` callback with metadata: token usage, latency (ms), model used.
-   `OnInferenceError` callback with error details.
-   These can be wired to application telemetry (e.g., OpenTelemetry, Application Insights).

### Progressive Enhancement (Fallback)
If the AI service is unavailable or returns an error:
-   **Smart Image:** Renders as a standard `<img>` tag with no `alt` text (or developer-supplied fallback `alt`).
-   **Smart Translate:** The translate button/icon is hidden or disabled. Original text remains untouched.
-   **Smart Summary:** The summary area displays a "Summary unavailable" message or is hidden entirely.

---

## 1. Smart Translate *(P1)*
**"Turn any form into a multilingual form — without .resx files"**

### The Problem
-   **Static vs. Dynamic:** Traditional `.resx` files miss user-generated content.
-   **Bad Workflows:** Users have to Copy → Tab Switch to Google Translate → Paste back, losing context and formatting.

### Component Vision
An **unobtrusive wrapper** `<SmartTranslate>` that enhances standard form inputs (TextAreas, Inputs) or text blocks. It adds a subtle "Translate" capability that opens a **Popover Interface**, ensuring the native input flow is never blocked.

### Capabilities & Scope
1.  **Interactive Popover Workflow:**
    -   **Trigger:** A subtle icon near the wrapped field, or a configurable hotkey (default: `Ctrl+Shift+T`, override via `Hotkey` parameter).
    -   **Popover UI:** Opens a dialog showing the draft translation of the selected text (or whole field).
    -   **Diff View:** The popover shows a visual diff (strikethrough old → highlighted new) to build trust for professional translators.
    -   **Refinement:** Users can manually edit the draft OR provide **natural language instructions** to the AI (e.g., *"Make it sound more professional"* or *"Use the term 'Device' instead of 'Unit'"*) to regenerate the translation.
    -   **Refinement Cap:** Maximum 3 refinement rounds per session. After that, a "Start Fresh" button resets the conversation to avoid token accumulation.
2.  **Flexible Application:**
    -   **Replace All:** Overwrites the entire field content with the approved translation.
    -   *(V2 — deferred)* **Insert at Cursor:** Deferred to V2 due to DOM complexity across browsers (especially `contenteditable`). See SmartMic precedent.
3.  **Language Auto-Detection:**
    -   The source language is auto-detected by the LLM — no manual selection required by the user.
4.  **Translation Memory / Glossary:**
    -   Developers can supply a `Glossary` parameter (`Dictionary<string, string>`) to enforce domain-specific terminology (e.g., `{"Unit" → "Device", "Discharge" → "Release"}`).
    -   The glossary is injected into the prompt as terminology constraints.
5.  **Context-Awareness:**
    -   Uses page context and specific user instructions to ensure domain accuracy (e.g., "discharge" in medical vs. military context).

---

## 2. Smart Summary *(P2)*
**"Cut reading time by 90% for support agents and admins"**

### The Problem
-   **Data Drowning:** Enterprise users (admins, support agents) spend massive amounts of time reading long email threads or logs.
-   **Mobile Friction:** Long texts are unreadable on mobile devices.

### Component Vision
A `<SmartSummary>` block that digests generic text inputs and outputs a structured, concise summary. The key differentiator vs. "just paste into ChatGPT" is **inline integration** — the summary appears within the app, next to the source data, without context-switching.

### Capabilities & Scope
1.  **Adaptive Summarization:**
    -   User can toggle between modes: **"TL;DR"**, **"Key Points"**, or **"Full Brief"**.
2.  **Source Linking:**
    -   Citations linking back to the part of the text used for the summary. **This is the primary differentiator and must ship in V1.**
3.  **Structured Extraction:**
    -   Can be configured to look for specific things (e.g., `FocusArea="Summarize only the decisions made in this meeting transcript"`).
4.  **Streaming:**
    -   Long summaries are streamed token-by-token to the UI for responsive UX.
5.  **Display Modes:**
    -   **Collapsible Card (Default):** "Show Summary" → Expands to show the AI output.
    -   **Inline Tooltip / Popover:** Hover over a truncated text (e.g., email subject in a list view) to see a 1-sentence TL;DR. Killer UX for dashboards.

---

## 3. Smart Image *(P3)*
**"Ship WCAG 2.2 AA compliant images with zero developer effort"**

### The Problem
- **Accessibility Compliance:** Manually writing `alt` text for user-uploaded content is rarely done, leading to WCAG non-compliance. The EU Accessibility Act (2025) carries real fines.
- **Visual Clutter:** User-uploaded images often have poor framing, requiring manual cropping that mistakenly cuts off key subjects (like faces).

### Component Vision
A self-contained `<SmartImage>` component that handles the "boring" but critical work of image management using Vision AI.

### Capabilities & Scope
1.  **Dedicated Vision Model:**
    -   Ability to configure a specific model (e.g., `google/gemini-flash-1.5` via OpenRouter) solely for image perception, distinct from the application's default text generation model.
2.  **Automatic Alt-Text Generation:**
    -   On upload or render, the component generates descriptive, context-aware `alt` text.
    -   *Example:* Uploading a picture of a broken pipe generates: *"Rusted metal pipe under a sink leaking water onto a wooden floor."*
3.  **Smart Cropping — Two-Tier Focal Point Detection:**
    -   **Tier 1 (Fast / Free / Client-Side):** Use the browser's `FaceDetector` API (Chromium) or a lightweight WASM model (e.g., BlazeFace ~2KB) for face/subject detection. Covers ~80% of use cases (portraits, product shots).
    -   **Tier 2 (LLM Fallback):** If Tier 1 returns no faces/subjects, fall back to the Vision model for complex scene saliency detection.
    -   Centers the viewport on the detected focal point, ensuring faces or products are never cut off in thumbnails.
4.  **Content Safety Check:**
    -   Opt-in via `EnableSafetyCheck="true"`.
    -   Configurable threshold: `SafetyThreshold="Low|Medium|High"` (default: `Medium`).
    -   Blurs or warns if the image contains NSFW or inappropriate content before displaying it.
