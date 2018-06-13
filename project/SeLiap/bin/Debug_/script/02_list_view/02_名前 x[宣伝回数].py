# -*- coding: utf-8 -*-

# data_grid_view         :
# none_effect_publicitys :
# publicitys             :
def View( data_grid_view, keishou, publicitys ):
    data_grid_view.Columns["Column1"].HeaderText = "宣伝者名";
    data_grid_view.Columns["Column2"].HeaderText = "コメント";
    for p in publicitys:
        counter = ""
        if p.counter > 1 :
            counter = " x" + str(p.counter)
        data_grid_view.Rows.Add( p.name + keishou + counter, p.comment )
