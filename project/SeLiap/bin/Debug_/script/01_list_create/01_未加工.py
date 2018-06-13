# -*- coding: utf-8 -*-

# none_effect_publicitys : 加工前のリスト
# publicitys             : 加工後のリスト(このリストを編集する)
def CreateList( none_effect_publicitys, publicitys ):
    for p in none_effect_publicitys:
        # pa.none_effect_publicitys とは別で管理するので実体を生成しておく
        publicitys.Add(p.Copy())
