var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory($"./src/Stars.Console/bin/{configuration}");
    CleanDirectory($"./src/Stars.Interface/bin/{configuration}");
    CleanDirectory($"./src/Stars.Services/bin/{configuration}");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreBuild("./src/Terradue.Stars.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });
});

// Task("Test")
//     .IsDependentOn("Build")
//     .Does(() =>
// {
//     DotNetCoreTest("./src/Example.sln", new DotNetCoreTestSettings
//     {
//         Configuration = configuration,
//         NoBuild = true,
//     });
// });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);