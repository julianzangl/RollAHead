# Kopf-System — Anleitung für Level-Design

Wie die Köpfe funktionieren und was ihr beim Bauen von Leveln beachten müsst.

---

## 1. Überblick

Der Spieler ist ein Zombie-Körper, der seinen **Kopf werfen** kann. Es gibt **4 Kopf-Typen**, jeder mit eigener Fähigkeit. Welcher Kopf gerade ausgerüstet ist, bestimmen **Pickups**, die ihr im Level platziert.

| Kopf | Script | Fähigkeit |
|------|--------|-----------|
| **Normal** (Zombie) | `Head.cs` | rollen, springen, Blöcke schieben |
| **Robot** | `RobotHead.cs` | an **Wänden klettern** (Linksklick halten an Wand) |
| **Slime** | `SlimeHead.cs` | **bounct automatisch** bei jeder Landung → für Jump-Passagen |
| **Fire** | `FireHead.cs` | **verbrennt** Objekte mit `Burnable`-Script bei Berührung |

Alle Köpfe teilen sich das Interface `IThrowableHead` — dadurch wirken **alle** Köpfe automatisch auf Buttons, Keys & Respawn.

---

## 2. Steuerung

| Taste | Aktion |
|-------|--------|
| WASD | Bewegen (Körper **und** geworfener Kopf) |
| **F halten + loslassen** | Kopf werfen (länger gedrückt = weiter, Wii-Golf-Kraft) |
| **F** (Tastendruck) | geworfenen Kopf zurückholen |
| Leertaste | Springen (Körper / Normal- & Fire-Kopf) |
| **Linksklick halten** | Robot-Kopf: an Wand klettern |

> Der **Slime-Kopf** bounct von selbst — kein Springen nötig, nur mit WASD steuern.

---

## 3. Köpfe ins Level bringen — Pickups

### a) Einmal-Pickups (für echte Level)
Verschwinden nach dem Aufsammeln. Pro Kopf ein eigenes Script:

- `RobotHeadPickup`
- `SlimeHeadPickup`
- `FireHeadPickup`

**So platzieren:**
1. Leeres GameObject (oder Modell) ins Level.
2. Einen Collider drauf (wird automatisch auf **Trigger** gesetzt).
3. Das passende Pickup-Script draufziehen.
4. Körper läuft rein → Kopf ausgerüstet → Pickup verschwindet. Dazu erscheint automatisch ein HUD-Tipp.

### b) Switch-Pickup (zum Testen / Hub-Bereiche)
`HeadSwitchPickup` — **verschwindet nicht**, beliebig oft nutzbar. Ein Feld **`Ability`** im Inspector (Normal / Robot / Slime / Fire). Ideal um mehrere Köpfe an einem Ort durchzuschalten.

> **Wichtig:** Pickups lösen nur aus, wenn der **Körper** reinläuft. Während der Kopf geworfen ist, ist der Körper deaktiviert → kann nicht aufsammeln. Pickups also dort hinstellen, wo der Körper hinkommt.

---

## 4. Fire-Kopf — brennbare Objekte

Damit der Feuerkopf ein Objekt zerstört:

1. Dem Objekt das **`Burnable`**-Script geben (ein Collider wird automatisch ergänzt, falls keiner da ist).
2. Collider **solide lassen** (kein Trigger).
3. Optional am `Burnable`: `Burn Delay` (kurze Verzögerung) und `Burn Effect Prefab` (VFX beim Verbrennen).

**Beachten:**
- Nur der **geworfene Feuerkopf** verbrennt — nicht der Körper, nicht andere Köpfe.
- Bewegt sich das Objekt (hat Rigidbody) und nutzt MeshCollider → **Convex** anhaken. Für einfache Formen lieber **BoxCollider**.

---

## 5. Eigene Kopf-Prefabs (Mesh / Material)

Jeder Spezialkopf hat sein eigenes Prefab mit Modell + Material + Script:

- `Assets/Prefabs/ThowableSlimeHead.prefab` (Script: `SlimeHead`)
- `Assets/Prefabs/ThowableBurningHead.prefab` (Script: `FireHead`)
- Robot: aktuell noch Greybox-Fallback (kein eigenes Prefab) — kann nachgereicht werden.

### ⚠️ Prefabs am Spieler zuweisen
Am **`HeadThrow`**-Component (auf dem Spieler) gibt es unter **"Ability Head Prefabs"** drei Slots:

| Slot | Prefab |
|------|--------|
| Slime Head Prefab | `ThowableSlimeHead` |
| Fire Head Prefab | `ThowableBurningHead` |
| Robot Head Prefab | leer (Fallback) bis Prefab existiert |

**Ist ein Slot leer → Greybox-Fallback** (Basis-Kopf wird zur Laufzeit umgebaut + eingefärbt). Nichts bricht, aber das schöne Prefab kommt erst raus, wenn es im Slot liegt. **Diese Slots in jeder Spieler-Instanz / jedem Level prüfen.**

### Anforderungen an ein Kopf-Prefab
- `Rigidbody` (Mass 1)
- `Collider` (z.B. SphereCollider, **kein** Trigger)
- Das passende Head-Script (`RobotHead` / `SlimeHead` / `FireHead`)
- Mesh + Material (+ optional Partikel)

> Tuning-Werte (Tempo, Sprungkraft, Bounce-Höhe, Kletter-Speed) stehen direkt auf dem Head-Script → **im Prefab im Inspector einstellbar.**

---

## 6. Was reagiert auf welchen Kopf?

| Element | reagiert auf |
|---------|--------------|
| `PressureButton` (Druckknopf) | Körper **und jeden** geworfenen Kopf |
| `KeyPickup` (Schlüssel) | Körper **und jeden** Kopf |
| `RespawnTrigger` (Todeszone) | Körper & jeden Kopf (setzt zurück) |
| `RobotHeadPickup` / Slime / Fire / Switch | nur **Körper** |
| `Checkpoint` | nur **Körper** |
| `Finish` (Levelende) | nur **Körper** |

→ Puzzles, bei denen ein Kopf einen Knopf gedrückt halten muss, funktionieren mit **jedem** Kopf. Level-Ende & Checkpoints erreicht nur der **Körper**.

---

## 7. Level-Design-Checkliste

- [ ] Pro Kopf-Fähigkeit ein passendes Pickup im Level platziert.
- [ ] `HeadThrow`-Prefab-Slots am Spieler zugewiesen (Slime/Fire).
- [ ] Brennbare Hindernisse haben `Burnable` + soliden Collider.
- [ ] Kletterwände sind für den Robot-Kopf erreichbar (Linksklick-Klettern).
- [ ] Slime-Passagen: genug vertikaler Platz zum Bouncen.
- [ ] `Finish`-Block am Levelende (Body läuft durch → Win-Screen + Timer-Stopp).
- [ ] Mindestens ein `RespawnPoint` / `Checkpoint` gesetzt.
- [ ] Szene in den **Build Settings** (sonst funktioniert „Neustart" im Win-Screen nicht).

---

## 8. Datei-Übersicht

```
Scripts/
  IThrowableHead.cs     Interface aller Köpfe
  Head.cs               Normal-Kopf
  RobotHead.cs          Klettern
  SlimeHead.cs          Bounce
  FireHead.cs           Verbrennen
  Burnable.cs           Markiert verbrennbare Objekte
  HeadThrow.cs          Wurf, Kraft-Charge, Prefab-Slots, equip-Logik
  RobotHeadPickup.cs    Einmal-Pickups …
  SlimeHeadPickup.cs
  FireHeadPickup.cs
  HeadSwitchPickup.cs   Wiederverwendbares Switch-Pickup (Enum)
  GameHUD.cs            HUD: Steuer-Hinweise, Kraft-Balken, Tipps, Timer, Win-Screen
```
