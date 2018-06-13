# -*- coding: utf-8 -*-

# ritch_text_box         :
# none_effect_publicitys :
# publicitys             :
def View( ritch_text_box, keishou, publicitys ):
    count = 0
    count_x = 0
    NAME_SIZE = 15
    Y_MAX = 4
    for p in publicitys:
        name = p.name + keishou
        # C#側で仕込んだ補助関数で、半角1文字、全角2文字で文字数を計算する
        name_len = assist.GetLenHAndF(name)

        # 残りが足りない場合は改行する
        nokori = NAME_SIZE * Y_MAX - count - name_len
        if nokori < 0:
            ritch_text_box.Text += "\n"
            count_x = 0
            count = 0

        # 以下のコードは強引に似たようなコードを書いているので、最適化できるはず...
        # 現状だと Y_MAX = 4 を増やす場合は、ここのコードを追加する必用がある
        if  name_len < NAME_SIZE :
            sp = ""
            for i in range(NAME_SIZE-name_len):
                sp += " "
            ritch_text_box.Text += name + sp
            count += NAME_SIZE
        elif name_len < NAME_SIZE * 2 :
            sp = ""
            for i in range(NAME_SIZE*2-name_len):
                sp += " "
            ritch_text_box.Text += name + sp
            count += NAME_SIZE * 2
        elif name_len < NAME_SIZE * 3 :
            sp = ""
            for i in range(NAME_SIZE*3-name_len):
                sp += " "
            ritch_text_box.Text += name + sp
            count += NAME_SIZE * 3
        elif name_len < NAME_SIZE * 4 :
            sp = ""
            for i in range(NAME_SIZE*4-name_len):
                sp += " "
            ritch_text_box.Text += name + sp
            count += NAME_SIZE * 4
        else:
            ritch_text_box.Text += name + " "
            count += name_len + 1

        count_x += 1

        if count_x >= Y_MAX:
            ritch_text_box.Text += "\n"
            count_x = 0
            count = 0
