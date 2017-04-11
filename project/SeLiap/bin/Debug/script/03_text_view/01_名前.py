# -*- coding: utf-8 -*-

# ritch_text_box         :
# none_effect_publicitys :
# publicitys             :
def View( ritch_text_box, keishou, publicitys ):
    for p in publicitys:
        ritch_text_box.Text += p.name + keishou + "\n"
