# Task List: Smart Translate, Smart Summary, Smart Image

This document tracks the implementation progress of the new Smart Components based on `docs/new-components-implementation-plan.md` and `docs/new-components-product-scope.md`.

## Phase 1: Foundation + Core Inference

### 1. SmartComponentBase (Prerequisite)
- [x] Create `SmartComponentBase` class in `SmartComponents.AspNetCore.Components`.
    - [x] Execution Modes (`Manual`, `Automatic`).
    - [x] Auto-trigger logic (`Debounce`, `OnBlur`).
    - [x] Rate limiting (`MinIntervalMs`, `MaxRequestsPerMinute`).
    - [x] Cancellation (`CancellationTokenSource`).
    - [x] Observability callbacks (`OnInferenceComplete`, `OnInferenceError`).
    - [x] Shared state management (`IsLoading`, `HasError`).

### 2. Core Inference Interfaces & Services
- [x] Define `ISmartTranslateInference`.
- [x] Define `ISmartSummaryInference`.
- [x] Define `ISmartImageInference`.
- [x] Implement `SmartTranslateInference` service.
    - [x] Auto-detect source language.
    - [x] Parameter: `glossary`.
    - [x] Parameter: `userInstructions`.
    - [x] Parameter: `pageContext`.
- [x] Implement `SmartSummaryInference` service.
    - [x] Streaming support (`IAsyncEnumerable`).
    - [x] Output with source citations.
- [x] Register new services in DI.
    - [x] Register `SmartTranslateInference`.
    - [x] Register `SmartSummaryInference` and map streaming endpoint.
    - [x] Register `SmartImageInference` and map endpoint.

## Phase 2: Frontend Components — Translate & Summary

### 1. SmartTranslate Component
- [x] Create `SmartTranslate.razor`.
    - [x] Inherit from `SmartComponentBase`.
    - [x] Implement Popover UI.
    - [x] Show Visual Diff (strikethrough -> highlight).
    - [x] Implement Refinement Loop (max 3 rounds).
    - [x] Implement Glossary injection.
- [x] Create `SmartTranslate.ts` (JS Interop).
    - [x] Selection handling.
    - [x] Replace text logic.

### 2. SmartSummary Component
- [x] Create `SmartSummary.razor`.
    - [x] Inherit from `SmartComponentBase`.
    - [x] Collapsible Card mode.
    - [x] Inline Tooltip mode.
    - [x] Visualizing source citations (Partial - Markdown only for now).
    - [x] Streaming UI updates.

## Phase 3: Smart Image + Polish

### 1. SmartImage Inference
- [x] Implement `SmartImageInference` service.
    - [x] Vision model support (`ModelOverride`).
    - [x] Safe search / Content safety check.
    - [x] Focal point detection (Tier 2 - JSON structure).

### 2. SmartImage Component
- [x] Create `SmartImage.razor`.
    - [x] Inherit from `SmartComponentBase`.
    - [x] Tier 1 Focal Point detection (Browser API / WASM).
    - [x] CSS `object-position` logic.
    - [x] Fallback handling (`alt` text).

### 3. Optimization & Testing
- [ ] Implement Caching for Image Inference (Currently simplified).
- [ ] Add End-to-End Tests for all new components.
- [ ] Verify Accessibility (WCAG 2.2 AA).
- [ ] Add Documentation / Examples.
