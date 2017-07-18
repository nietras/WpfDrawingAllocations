$vstestconsolepath = $env:VS140COMNTOOLS + "..\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
$testRoot = "build\Tests\"

$testDlls = Get-ChildItem -Path $testRoot -File -Recurse -Include *Test.dll

& $vstestconsolepath $testDlls '/Platform:x86' '/Parallel'