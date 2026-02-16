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
- [ ] Implement `SmartSummaryInference` service.
    - [ ] Streaming support (`IAsyncEnumerable`).
    - [ ] Output with source citations.
- [ ] Register new services in DI.
    - [x] Register `SmartTranslateInference`.

## Phase 2: Frontend Components — Translate & Summary

### 1. SmartTranslate Component
- [ ] Create `SmartTranslate.razor`.
    - [ ] Inherit from `SmartComponentBase`.
    - [ ] Implement Popover UI.
    - [ ] Show Visual Diff (strikethrough -> highlight).
    - [ ] Implement Refinement Loop (max 3 rounds).
    - [ ] Implement Glossary injection.
- [ ] Create `SmartTranslate.ts` (JS Interop).
    - [ ] Selection handling.
    - [ ] Replace text logic.

### 2. SmartSummary Component
- [ ] Create `SmartSummary.razor`.
    - [ ] Inherit from `SmartComponentBase`.
    - [ ] Collapsible Card mode.
    - [ ] Inline Tooltip mode.
    - [ ] Visualizing source citations.
    - [ ] Streaming UI updates.

## Phase 3: Smart Image + Polish

### 1. SmartImage Inference
- [ ] Implement `SmartImageInference` service.
    - [ ] Vision model support (`ModelOverride`).
    - [ ] Safe search / Content safety check.
    - [ ] Focal point detection (Tier 2).

### 2. SmartImage Component
- [ ] Create `SmartImage.razor`.
    - [ ] Inherit from `SmartComponentBase`.
    - [ ] Tier 1 Focal Point detection (Browser API / WASM).
    - [ ] CSS `object-position` logic.
    - [ ] Fallback handling (`alt` text).

### 3. Optimization & Testing
- [ ] Implement Caching for Image Inference.
- [ ] Add End-to-End Tests for all new components.
- [ ] Verify Accessibility (WCAG 2.2 AA).
- [ ] Add Documentation / Examples.
