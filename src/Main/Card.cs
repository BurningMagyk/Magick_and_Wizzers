using Godot;
using System;

namespace Main {
public class Card {
  
  public string Name { get; set; }
  public Stats Stats { get; set; }
  public Texture2D Illustration { get; set; }

  public Card(string name, Stats stats, Texture2D illustration) {
    Name = name;
    Stats = stats;
    Illustration = illustration;
  }

  public override string ToString() {
    return $"Card: {Name}";
  }

  public static Card CreateRandomCard() {
    return new Card(
      "Random Name",
      Stats.CreateRandom(),
      GD.Load<Texture2D>("res://resources/card_textures/card_art/Trololololo_Road.png")
    );
  }
}}
