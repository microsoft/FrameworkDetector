---
id: TLA
title: Framework Detection Template
description: Template for authoring framework detection documentation pages.
website: https://frameworkdetector.example.com/docs
source: https://github.com/Organization/Repository
category: Component, Framework, Library, ProgrammingLanguage
keywords: Framework Detector, ...
ms.date: MM/DD/YYYY # Delete this comment, date last updated
author: githubusername
---

# FrameworkName

## Summary

<!--

Provide a high-level description of FrameworkName and include a link to its public website.

If different (major) versions of FrameworkName have considerably different specifics (detection methods, language/OS support, etc.) consider creating a separate framework doc and linking to it here.

-->

### Languages

<!--

Provide both the programming language(s) that FrameworkName is developed in, as well as the the target programming language(s) that app developers are expected to use to access FrameworkName when building their app.

If FrameworkName was built with a programming language that provides some common ABI (WinRT, .NET, C) to support an indefinite number of language (projections), it's okay to limit the list to the most prominent languages and/or those that have official samples.

-->

### OS Support

<!--

Provide the OS families that FrameworkName supports (Windows, Linux, macOS, iOS, Android, etc.). Include any information about minimum supported versions (10, 11, etc). 

-->

### Dependencies

<!--

List (and link) any frameworks that FrameworkName itself requires (whether its detection is supported by FrameworkDetector or not).

-->

### Canonical Apps

<!--

List (and link) at least one example retail app that uses FrameworkName in the expected manner, as a "canonical" app. Preferably one that is free to download an run.

If there are multiple different expected ways to consume FrameworkName (i.e. that may require different methods to detect FrameworkName), include those as well.

-->

## How to Detect

<!--

This section covers how to detect FrameworkName in a given app, and should match FrameworkDetector's expected implementation.

For each section include not just how to correctly identify the presence of FrameworkName, but also how to detect/calculate common metadata about FrameworkName's use by the app (such as version/build used, etc.).

If applicable, include any useful, framework-specific metadata that can be detected/calculated (such as feature flags, modes, etc.) and how to detect it.

Perhaps just as important, if there are popular methods for detection that are incorrect and/or problematic (due to causing false positives, being generally unreliable, etc.) be sure to mention those methods and explain why they should not be used.

-->

### Runtime Detection

<!--

Describe how to detect the presence of FrameworkName from an already running app process.

This often includes looking for a specific module or DLL from FrameworkName in the list of the target app process's loaded modules.

If some or all of the correct information can be reliably detected by tracing the file path of the executable / modules in the target app process and then performing a more stative detection, it's okay to document it in the following section and just reference it here.

-->

### Static Detection

<!--

Describe how to detect the presence of FrameworkName strictly from files on disk, without launching the target app. While we expect runtime detection to be more reliable and/or informative, in many cases, an evaluation of the app's contents on disk may be cheaper and still useful.

This can include instructions for detecting the presence of FrameworkName in Store packages (directly or via manifest/config files), and/or unpackaged apps packaged (including the contents of their current directory).

-->

## Resources

<!--

Include an list of links to any relevant online resources about FrameworkName (main site, docs, samples, "how to detect" SO posts, etc.).

-->
