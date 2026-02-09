using Godot;
using System;
using System.Collections.Generic;

namespace Match {
public class Player {
    private List<Piece> mMasters = new List<Piece>();

    public string Name { get; private set; }
    public Main.Card MasterCard { get; private set; }
    public bool Alive { get; set; }
    public Player(string name, Main.Card masterCard) {
        Name = name;
        MasterCard = masterCard;
        Alive = true;
    }

    public void AddMaster(Piece master) {
        mMasters.Add(master);
    }
}
}
