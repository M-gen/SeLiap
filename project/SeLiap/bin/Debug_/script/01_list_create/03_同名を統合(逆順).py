# -*- coding: utf-8 -*-

# none_effect_publicitys : (src) 加工前のリスト
# publicitys             : (dst) 加工後のリスト(このリストを編集する)
def CreateList( none_effect_publicitys, publicitys  ):
    for p in reversed(none_effect_publicitys):
        is_skip = False

        for p2 in publicitys:
            if p.name == p2.name:
                p2.counter += 1
                if (p.comment != ""):
                    if (p2.comment != ""):
                        p2.comment += "\n" + p.comment
                    else:
                        p2.comment = p.comment
                is_skip = True
                break

        if is_skip:
            continue

        # pa.none_effect_publicitys とは別で管理するので実体を生成しておく
        publicitys.Add(p.Copy())
