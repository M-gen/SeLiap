rem Debugの確認環境にあるデータを、Releaseにコピー(ミラー)します
robocopy "../project/SeLiap\bin\Debug\data" "../project/SeLiap\bin\Release\data" /MIR /R:0 /W:0 /NP /XJD /XJF

rem 不足している可能性のある DXライブラリのDLLをコピーしておく
copy "../project/SeLiap\bin\Debug\DxLib.dll" "../project/SeLiap\bin\Release\DxLib.dll" /Y
copy "../project/SeLiap\bin\Debug\DxLib_x64.dll" "../project/SeLiap\bin\Release\DxLib_x64.dll" /Y
copy  "../project/SeLiap\bin\Debug\DxLibDotNet.dll" "../project/SeLiap\bin\Release\DxLibDotNet.dll" /Y