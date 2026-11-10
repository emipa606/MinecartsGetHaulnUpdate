# GitHub Copilot Instructions for Minecarts - Get Haul'n Update (Continued)

## Mod Overview and Purpose

**Minecarts - Get Haul'n Update (Continued)** is an update to the Minecarts mod, originally created by Compilatron. This mod allows players to move items around their colonies using minecarts. It includes revamped vehicle physics, connectable minecart trains, automatic pickup and drop-off points, and general balance changes.

## Key Features and Systems

### Minecarts
- **Build and launch**: Players can build minecarts from the "rails" tab of the architecture menu. Minecarts can be pushed to launch, and pawns can be assigned to launch them.
- **Transport items**: Minecarts can carry items like transport pods.
- **Connectable trains**: Minecarts can be connected to form trains.
- **Automatic pickup and drop-off**: Players can set up automatic pickup and drop-off points for minecarts.

### Vehicle Physics
- **Revamped physics**: Improved vehicle physics for a more realistic and enjoyable experience.

### Designation Categories
- **New categories**: Added new designation categories for easier management of minecart-related tasks.

### Jobs
- **New jobs**: Added new jobs for minecart-related tasks.

## Coding Patterns and Conventions

### C# Files
- **Class Names**: Use PascalCase for class names (e.g., `Building_Minecart`).
- **Method Names**: Use camelCase for method names (e.g., `DoRailStep`).
- **Constants and Enums**: Use PascalCase for constants and enums (e.g., `DefOfs`).
- **Properties**: Use PascalCase for property names (e.g., `Speed`).

### XML Files
- **Tag Names**: Use PascalCase for tag names (e.g., `ThingDef`).
- **Attribute Names**: Use camelCase for attribute names (e.g., `minifiedDef`).
- **Comments**: Use `<!-- -->` for comments in XML files.

## XML Integration

The mod uses XML files to define various game elements such as buildings, jobs, and designation categories. These files are located in the `Defs` directory of the mod.

### Example XML File: Buildings_Minecart.xml
xml
<Defs>
  <ThingDef>
    <defName>Building_Minecart</defName>
    <label>Minecart</label>
    <description>A cart that can carry items and be moved around.</description>
    <thingCategories>
      <li>Buildings</li>
    </thingCategories>
    <costList>
      <Steel>10</Steel>
    </costList>
    <comps>
      <li>
        <compClass>CompMinecart</compClass>
      </li>
    </comps>
  </ThingDef>
</Defs>


## Harmony Patching

The mod uses Harmony to patch existing game code to add or modify functionality. Harmony patches are located in the `Patches` directory of the mod.

### Example Harmony Patch: LaunchCartPatch.cs
csharp
using HarmonyLib;
using RimWorld;
using Verse;

namespace Minecart
{
    [HarmonyPatch(typeof(JobDriver_LaunchCart), "MakeNewToils")]
    public static class LaunchCartPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(JobDriver_LaunchCart __instance)
        {
            // Custom code to modify the behavior of launching a cart
            Log.Message("Launching minecart with custom code");
            return true;
        }
    }
}


## Suggestions for Copilot

### General Suggestions
- **Code Completion**: Use Copilot to complete code snippets and reduce manual typing.
- **Code Suggestions**: Use Copilot to suggest code patterns and conventions.
- **Error Handling**: Use Copilot to suggest error handling code.

### C# Suggestions
- **Method Implementations**: Use Copilot to suggest method implementations.
- **Property Definitions**: Use Copilot to suggest property definitions.
- **Class Definitions**: Use Copilot to suggest class definitions.

### XML Suggestions
- **Tag Suggestions**: Use Copilot to suggest XML tags and attributes.
- **Comment Suggestions**: Use Copilot to suggest XML comments.

### Harmony Patching Suggestions
- **Patch Suggestions**: Use Copilot to suggest Harmony patches.
- **Prefix/Suffix Suggestions**: Use Copilot to suggest Harmony prefix/suffix methods.

By following these instructions and utilizing Copilot, you can efficiently develop and maintain the Minecarts - Get Haul'n Update (Continued) mod.

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

