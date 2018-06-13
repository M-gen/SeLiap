# -*- coding: utf-8 -*-

# ritch_text_box         :
# none_effect_publicitys :
# publicitys             :
def View( ritch_text_box, keishou, publicitys ):
    count = 0
    for p in publicitys:
        name = p.name + keishou
        # C#側で仕込んだ補助関数で、半角1文字、全角2文字で文字数を計算する
        name_len = assist.GetLenHAndF(name)
        count += name_len
        if count > 30:
            count = name_len + 1
            ritch_text_box.Text += "\n"
            ritch_text_box.Text += name
        else:
            count += 1
            if ritch_text_box.Text=="":
                ritch_text_box.Text += name
            else:
                ritch_text_box.Text += " " + name
