rem Debugの確認環境にあるデータを、Releaseにコピー(ミラー)します
robocopy "../project/SeLiap\bin\Debug\script" "../project/SeLiap\bin\Release\script" /MIR /R:0 /W:0 /NP /XJD /XJF

rem 不足している可能性のある DXライブラリのDLLをコピーしておく
rem copy "../project/SeLiap\bin\Debug\x" "../project/SeLiap\bin\Release\x" /Y