# -*- coding: utf-8 -*-

# data_grid_view         :
# none_effect_publicitys :
# publicitys             :
def CreateList( data_grid_view, keishou, none_effect_publicitys, publicitys  ):
    data_grid_view.Columns["Column1"].HeaderText = "宣伝者名";
    data_grid_view.Columns["Column2"].HeaderText = "コメント";
    for p in publicitys:
        data_grid_view.Rows.Add( p.name + keishou, p.comment )
