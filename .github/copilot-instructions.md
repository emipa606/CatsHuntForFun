# GitHub Copilot Instructions for Cats Hunt For Fun Mod

## Mod Overview and Purpose
Cats Hunt For Fun is a RimWorld mod that introduces a new activity for cats where they hunt for fun, regardless of whether they are hungry or not. The mod enhances the gameplay experience by adding an engaging activity for cats, making them more dynamic and entertaining.

## Key Features and Systems
- **New Hunting Activity**: Cats will have a new activity to hunt when they are not performing any other tasks.
- **Surprise Attack**: The hunting attack is always a surprise attack, reducing the risk for the cat to get hurt.
- **Health Conditions**: Cats will not hunt when they are hurt or downed. They will also only attack prey that is downed or killed.
- **Gift Option**: If the gift option is active, cats can take a prey in any health state and leave it as a gift to their owner.
- **Configurable Options**: The mod offers various settings to customize the hunting behavior of cats, such as range to search for prey, likelihood of attacking prey, bringing prey as a gift, relative body-size to attack, hunting only in the home area, not hunting player colony animals, and wild animals hunting for fun.

## Coding Patterns and Conventions
- **C# Coding Style**: Follow the standard RimWorld C# coding style, including naming conventions, comments, and structure.
- **XML Defs**: Use XML definitions to define new jobs, think trees, and patches.
- **Mod Settings**: Implement mod settings using the RimWorld settings framework to allow players to customize the mod behavior.

## XML Integration
- **JobDefs**: Define new jobs for cats in the `JobDefs` XML files.
- **ThinkTreeDefs**: Define new think tree nodes for the hunting and gift-giving behavior in the `ThinkTreeDefs` XML files.
- **Patches**: Create Harmony patches in the `Patches` XML files to modify existing RimWorld behavior to support the new hunting activity.

## Harmony Patching
- **Harmony Patches**: Use Harmony patches to modify existing RimWorld methods and add new functionality without altering the original codebase.
- **Example Patch**: An example patch might look like this:

xml
<HarmonyPatch>
  <Type>Verse.AI.ThinkNode_Conditional</Type>
  <Method>HasJobGotTarget</Method>
  <Prefix>Patch_HasJobGotTarget</Prefix>
</HarmonyPatch>


## Suggestions for Copilot
- **C# Code Snippets**: Generate C# code snippets for implementing new jobs, think tree nodes, and mod settings.
- **XML Def Examples**: Provide XML def examples for creating new jobs, think trees, and patches.
- **Harmony Patch Templates**: Offer Harmony patch templates to help developers create custom patches for modifying RimWorld behavior.
- **Mod Setting UI**: Assist in creating user interface elements for the mod settings window.

---

This document serves as a guide for developers working on the Cats Hunt For Fun mod, providing an overview of the mod's purpose, key features, coding patterns, XML integration, and Harmony patching. GitHub Copilot can be used to generate code snippets, XML def examples, and Harmony patch templates to streamline the development process.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

