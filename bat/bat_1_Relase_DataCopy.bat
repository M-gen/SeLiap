rem Debug�̊m�F���ɂ���f�[�^���ARelease�ɃR�s�[(�~���[)���܂�
robocopy "../project/SeLiap\bin\Debug\script" "../project/SeLiap\bin\Release\script" /MIR /R:0 /W:0 /NP /XJD /XJF

rem �s�����Ă���\���̂��� DX���C�u������DLL���R�s�[���Ă���
rem copy "../project/SeLiap\bin\Debug\x" "../project/SeLiap\bin\Release\x" /Y