# Audit des ConflictZones de MVP_Run -- Story 5.35 (2026-10-05)

Modele `v4:46e91057b9f6e8a970fe7952639805a6`, decisions `2e290408351c1b6d8af536d76441974b3bcdb30f814cb692e173c95a41f58e0f`, commit `bb55ae9b54be5bffa0eb5df77b11660ea224c5b2`. Lecture seule : aucune donnee modifiee.

## Methode

- Distance minimale entre les axes (echantillons `Samples`, pas max 0,104 m, erreur <= ~0,05 m) des deux mouvements de chaque zone. Classification et raison lues dans `MVP_Run.road-authoring.json`.
- Demi-gabarit gonfle de reference : demi-largeur 1.03 + marge 0.25 + a_e 0,34 = 1,62 m, soit 3,24 m pour deux vehicules alignes sur leurs axes ; en virage, le cap decale (pose cinematique) elargit l'emprise laterale.
- Mecanisme (`AutomatedPairDecisionPolicy.cs:491-497`) : une paire `Candidate` (enveloppes grossieres qui se recouvrent) sans temoin ponctuel de contact est acceptee `ConservativeConflict` (`continuous-contact-possible`). La politique ne tente jamais de prouver la separation des rectangles orientes pour une paire `Candidate`.
- Une distance d'axes n'est PAS une preuve de separation : elle indique seulement les paires a soumettre a une preuve.

## Synthese

| Groupe | Zones | Lecture |
|---|---|---|
| A (axes >= 7 m) | 16 | tres probablement faux conflit : axes separes de plus de deux fois le demi-gabarit gonfle |
| B (axes ~4 m, voies voisines) | 54 | a prouver : voies voisines de sens oppose ; separation attendue positive en ligne droite (4,0 - 3,24 = 0,76 m), incertaine en virage |
| C (axes < 0,1 m, croisement reel) | 24 | conflit plausible (les axes se croisent), classe par prudence |
| P (contact prouve) | 42 | conflit reel (temoin de recouvrement) |

| Carrefour | P | A | B | C |
|---|---|---|---|---|
| Intersection_Center_Crossroads | 18 | 0 | 18 | 12 |
| Roundabout_NorthEast | 3 | 4 | 3 | 0 |
| Roundabout_NorthWest | 3 | 4 | 3 | 0 |
| Roundabout_SouthEast | 3 | 4 | 3 | 0 |
| Roundabout_SouthWest | 3 | 4 | 3 | 0 |
| TJunction_East | 3 | 0 | 6 | 3 |
| TJunction_North | 3 | 0 | 6 | 3 |
| TJunction_South | 3 | 0 | 6 | 3 |
| TJunction_West | 3 | 0 | 6 | 3 |

Consequence : dans chaque T, les 12 paires de mouvements d'approches differentes sont toutes en conflit (aucune paire inter-approches compatible) ; la croix n'en a que 6 compatibles sur 54. En giratoire, une entree est en conflit avec l'entree d'un autre bras (7,38 m) et avec la sortie de son propre bras (3,99 m).

## Paires concernees (groupes A et B)

| Carrefour | Groupe | Axes (m) | Zone | Mouvement 1 | Mouvement 2 |
|---|---|---|---|---|---|
| Intersection_Center_Crossroads | B | 4.00 | `4037740c6f0de354c5000388516f6185` | Junction_FromNorth -> Connector_East_Out (gauche) | Junction_FromEast -> Connector_North_Out (droite) |
| Intersection_Center_Crossroads | B | 4.00 | `4147689d782f388b4c54ff1a77e72ca5` | Junction_FromSouth -> Connector_North_Out (tout droit) | Junction_FromNorth -> Connector_West_Out (droite) |
| Intersection_Center_Crossroads | B | 4.00 | `417f3abf3c05390e7a87ecfd98a6a481` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromEast -> Connector_West_Out (tout droit) |
| Intersection_Center_Crossroads | B | 4.00 | `434cfd5ad42d31287fd0fc4be036f3b3` | Junction_FromSouth -> Connector_North_Out (tout droit) | Junction_FromWest -> Connector_South_Out (droite) |
| Intersection_Center_Crossroads | B | 4.00 | `44eb41658994bf0d068556234e7c7491` | Junction_FromWest -> Connector_South_Out (droite) | Junction_FromSouth -> Connector_West_Out (gauche) |
| Intersection_Center_Crossroads | B | 4.00 | `4505708aa91fbff05b77dbe422d6e1b5` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromWest -> Connector_South_Out (droite) |
| Intersection_Center_Crossroads | B | 4.00 | `454750014c7f7da3a45a39cb6189b982` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromEast -> Connector_North_Out (droite) |
| Intersection_Center_Crossroads | B | 4.00 | `45f53e49188f42e47576c9788d6c899b` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromEast -> Connector_South_Out (gauche) |
| Intersection_Center_Crossroads | B | 4.00 | `47153008967b6178e0b67b3b0dd7a289` | Junction_FromEast -> Connector_North_Out (droite) | Junction_FromWest -> Connector_East_Out (tout droit) |
| Intersection_Center_Crossroads | B | 4.00 | `4a4ed5b2470adc5ba3350df9cc85768e` | Junction_FromNorth -> Connector_West_Out (droite) | Junction_FromWest -> Connector_North_Out (gauche) |
| Intersection_Center_Crossroads | B | 4.00 | `4a73e9da4c0ce92c90703379e30d7b9f` | Junction_FromNorth -> Connector_West_Out (droite) | Junction_FromWest -> Connector_East_Out (tout droit) |
| Intersection_Center_Crossroads | B | 4.00 | `4a8d1f62800182ac0fade27255a39190` | Junction_FromNorth -> Connector_South_Out (tout droit) | Junction_FromSouth -> Connector_North_Out (tout droit) |
| Intersection_Center_Crossroads | B | 4.00 | `4b29ef3394ffa2df90a10e235b95a59c` | Junction_FromWest -> Connector_South_Out (droite) | Junction_FromNorth -> Connector_West_Out (droite) |
| Intersection_Center_Crossroads | B | 4.00 | `4bb1ec8d6203ad58929013286d06278b` | Junction_FromNorth -> Connector_South_Out (tout droit) | Junction_FromSouth -> Connector_East_Out (droite) |
| Intersection_Center_Crossroads | B | 4.00 | `4c05c15b5871449b27880b4c5c1f2e92` | Junction_FromNorth -> Connector_South_Out (tout droit) | Junction_FromEast -> Connector_North_Out (droite) |
| Intersection_Center_Crossroads | B | 4.00 | `4d643d916a884b3c1233a665b66d89bc` | Junction_FromNorth -> Connector_West_Out (droite) | Junction_FromEast -> Connector_North_Out (droite) |
| Intersection_Center_Crossroads | B | 4.00 | `4f93788f6768a9c8d22bd432fff01490` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromWest -> Connector_South_Out (droite) |
| Intersection_Center_Crossroads | B | 4.00 | `415e54e20484285ef32cb5ede00b61ae` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromWest -> Connector_East_Out (tout droit) |
| Roundabout_NorthEast | A | 7.38 | `458eb40d533eff27d37e8ed61c6eaabb` | Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) |
| Roundabout_NorthEast | A | 7.38 | `4cbd976887b4f64454c372023ad131a2` | Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Ring_Split_South -> Connector_South_Out (sortie d'anneau) |
| Roundabout_NorthEast | A | 7.38 | `47b2bdca8b4d502a90968e846cd49abb` | Connector_West_In -> Ring_Merge_West (entree d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| Roundabout_NorthEast | A | 7.38 | `4b53383c5c1fcf5fddb5b7ee011ed78a` | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| Roundabout_NorthEast | B | 3.99 | `415d14c83ddadbaca56eba4406bcf2b8` | Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Connector_West_In -> Ring_Merge_West (entree d'anneau) |
| Roundabout_NorthEast | B | 3.99 | `44434fa7a74b378cd42ed08c42fe8692` | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) |
| Roundabout_NorthEast | B | 3.99 | `49b398688f7efc5feae64cccd10773ac` | Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| Roundabout_NorthWest | A | 7.38 | `45221b336e33b670c8623e386ad6bf8d` | Connector_South_In -> Ring_Merge_South (entree d'anneau) | Connector_West_In -> Ring_Merge_West (entree d'anneau) |
| Roundabout_NorthWest | A | 7.38 | `45c9ea1a463d131a176bf4c5516872a3` | Connector_South_In -> Ring_Merge_South (entree d'anneau) | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) |
| Roundabout_NorthWest | A | 7.38 | `470ef9a2fd6c8d61f93092fe1646ba97` | Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Ring_Split_West -> Connector_West_Out (sortie d'anneau) |
| Roundabout_NorthWest | A | 7.38 | `4f1c595ed26310a51f9f60a0f3ead586` | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Ring_Split_West -> Connector_West_Out (sortie d'anneau) |
| Roundabout_NorthWest | B | 3.99 | `45c7253beaef1bef98463cee884919a4` | Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| Roundabout_NorthWest | B | 3.99 | `43e5a24dac93246333d9faf399592587` | Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Connector_West_In -> Ring_Merge_West (entree d'anneau) |
| Roundabout_NorthWest | B | 3.99 | `47114f641089fae85c0ee8305935fdaa` | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) |
| Roundabout_SouthEast | A | 7.38 | `433fa627c90e7aef2af3fd986d2faa8d` | Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) |
| Roundabout_SouthEast | A | 7.38 | `45168e2b5ecebf3c89b80036bbddd0af` | Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Ring_Split_South -> Connector_South_Out (sortie d'anneau) |
| Roundabout_SouthEast | A | 7.38 | `4269dde366f786c6fd4e279786aefc94` | Connector_West_In -> Ring_Merge_West (entree d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| Roundabout_SouthEast | A | 7.38 | `433af752017a55720032fc063252c999` | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| Roundabout_SouthEast | B | 3.99 | `48fd616bb1789845fc2043ad1f0ba7a5` | Connector_West_In -> Ring_Merge_West (entree d'anneau) | Ring_Split_West -> Connector_West_Out (sortie d'anneau) |
| Roundabout_SouthEast | B | 3.99 | `4fa896603af971df28394d41512e83b8` | Connector_South_In -> Ring_Merge_South (entree d'anneau) | Ring_Split_South -> Connector_South_Out (sortie d'anneau) |
| Roundabout_SouthEast | B | 3.99 | `406efc13fc2d5de520cbe34197314db3` | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) |
| Roundabout_SouthWest | A | 7.38 | `4b38f8659ad8dac11a7d0a0b53fc47b5` | Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) |
| Roundabout_SouthWest | A | 7.38 | `4fedf986cee6f96bf250c5b1f2439e82` | Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Ring_Split_South -> Connector_South_Out (sortie d'anneau) |
| Roundabout_SouthWest | A | 7.38 | `45f7adaf82bc052cf599183a4c402484` | Connector_South_In -> Ring_Merge_South (entree d'anneau) | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) |
| Roundabout_SouthWest | A | 7.38 | `4dbd9c73370da2e3405d8328dacd9d99` | Connector_West_In -> Ring_Merge_West (entree d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| Roundabout_SouthWest | B | 3.99 | `450099ba966907d2e411d1b20b54ae99` | Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Connector_West_In -> Ring_Merge_West (entree d'anneau) |
| Roundabout_SouthWest | B | 3.99 | `42b83e7c01b9cbacc9919a1bb7e9e68c` | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) |
| Roundabout_SouthWest | B | 3.99 | `4a4250039f40a26ff5b4051a197c149d` | Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| TJunction_East | B | 4.00 | `436005183adb61a537157827df3372be` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_East | B | 4.00 | `43ebda8c3f20a8ad55ad4a9628e3edab` | Junction_FromWest -> Connector_South_Out (droite) | Junction_FromEast -> Connector_West_Out (tout droit) |
| TJunction_East | B | 4.00 | `46a75b0c04792a9ac17415da45f3c98f` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromWest -> Connector_South_Out (droite) |
| TJunction_East | B | 4.00 | `489e099514f065e68f19cf4dbfe350b2` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromEast -> Connector_West_Out (tout droit) |
| TJunction_East | B | 4.00 | `499ace82c622f81cf600719848bae693` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromWest -> Connector_South_Out (droite) |
| TJunction_East | B | 4.00 | `49f9a6d925302128b2c14ee2328fabb6` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromEast -> Connector_South_Out (gauche) |
| TJunction_North | B | 4.00 | `40bd2f68ba2bc3ff9535b711aea9998f` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_North | B | 4.00 | `436441bec58608acbc6fb187181a6bbe` | Junction_FromWest -> Connector_South_Out (droite) | Junction_FromSouth -> Connector_East_Out (droite) |
| TJunction_North | B | 4.00 | `437c2f58c60a75b9a723a64035b160b2` | Junction_FromWest -> Connector_South_Out (droite) | Junction_FromEast -> Connector_West_Out (tout droit) |
| TJunction_North | B | 4.00 | `43c7c6dff15ddd8ea442f3b4cb85a884` | Junction_FromWest -> Connector_South_Out (droite) | Junction_FromSouth -> Connector_West_Out (gauche) |
| TJunction_North | B | 4.00 | `49e18ec3cedc8169b621139413c88eab` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromSouth -> Connector_East_Out (droite) |
| TJunction_North | B | 4.00 | `4a85cf73649c4ac79f7a21112c98daa3` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromSouth -> Connector_East_Out (droite) |
| TJunction_South | B | 4.00 | `46a27562fc7ef73899775b45767338b1` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromWest -> Connector_South_Out (droite) |
| TJunction_South | B | 4.00 | `4b09a0f12076cb983208a9d922a545b7` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_South | B | 4.00 | `4ba61b4456363d785c934bbc3dbb98aa` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromSouth -> Connector_East_Out (droite) |
| TJunction_South | B | 4.00 | `4c3765f4eeddd6c6be43e3c0c8c83182` | Junction_FromWest -> Connector_South_Out (droite) | Junction_FromSouth -> Connector_East_Out (droite) |
| TJunction_South | B | 4.00 | `4dc9c6211a49ca8f0980f973af7a1bac` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromSouth -> Connector_East_Out (droite) |
| TJunction_South | B | 4.00 | `4fe75f8b969a1fadc554e6cd737aee9c` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromWest -> Connector_South_Out (droite) |
| TJunction_West | B | 4.00 | `40fd4fd762a2c46f4f8087d3f9a597b2` | Junction_FromWest -> Connector_South_Out (droite) | Junction_FromSouth -> Connector_West_Out (gauche) |
| TJunction_West | B | 4.00 | `465d5eec7c97132ad85c3ac363cca48e` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_West | B | 4.00 | `4905f97ebed6b813a1b1ba7b9fc594bb` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromWest -> Connector_South_Out (droite) |
| TJunction_West | B | 4.00 | `4b49ca4b2117973d8355dbf1660471b3` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromSouth -> Connector_East_Out (droite) |
| TJunction_West | B | 4.00 | `4ce031f1b7b682c6e75c39e87a7b4ab7` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromSouth -> Connector_East_Out (droite) |
| TJunction_West | B | 4.00 | `4fdf379316413439f08ee9573e94b995` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromWest -> Connector_South_Out (droite) |

## Autres zones (P et C), pour reference

| Carrefour | Groupe | Axes (m) | Zone | Mouvement 1 | Mouvement 2 |
|---|---|---|---|---|---|
| Intersection_Center_Crossroads | C | 0.03 | `4ac63be5d9fb0655462900ee2188a088` | Junction_FromNorth -> Connector_East_Out (gauche) | Junction_FromEast -> Connector_West_Out (tout droit) |
| Intersection_Center_Crossroads | C | 0.03 | `4b4cb6b3d76549de0935338e2b66aea9` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromWest -> Connector_East_Out (tout droit) |
| Intersection_Center_Crossroads | C | 0.03 | `490e9e2ea4bef88bf4a8ee14433a429b` | Junction_FromNorth -> Connector_East_Out (gauche) | Junction_FromSouth -> Connector_North_Out (tout droit) |
| Intersection_Center_Crossroads | C | 0.03 | `4b168164d6d552a9e382264821c7dcbe` | Junction_FromNorth -> Connector_South_Out (tout droit) | Junction_FromSouth -> Connector_West_Out (gauche) |
| Intersection_Center_Crossroads | C | 0.03 | `452cc0b3f8ae471194e7907f1351d3ad` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromWest -> Connector_North_Out (gauche) |
| Intersection_Center_Crossroads | C | 0.03 | `4a6c823b85b757d2f3505c8789aa528c` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromWest -> Connector_East_Out (tout droit) |
| Intersection_Center_Crossroads | C | 0.03 | `41143c56ca46d5c88f92b78870868c87` | Junction_FromNorth -> Connector_South_Out (tout droit) | Junction_FromWest -> Connector_North_Out (gauche) |
| Intersection_Center_Crossroads | C | 0.03 | `4d99e930a4f47c2ed260aed77cac1f8d` | Junction_FromSouth -> Connector_North_Out (tout droit) | Junction_FromEast -> Connector_South_Out (gauche) |
| Intersection_Center_Crossroads | C | 0.02 | `4645efe76bbc8c5cda09e53a759b85b9` | Junction_FromNorth -> Connector_East_Out (gauche) | Junction_FromEast -> Connector_South_Out (gauche) |
| Intersection_Center_Crossroads | C | 0.02 | `4c9b462c1b8587eb241406b3a9d11683` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromWest -> Connector_North_Out (gauche) |
| Intersection_Center_Crossroads | C | 0.02 | `42665e8c1aa6739044cecc21fb9f79be` | Junction_FromNorth -> Connector_East_Out (gauche) | Junction_FromWest -> Connector_North_Out (gauche) |
| Intersection_Center_Crossroads | C | 0.02 | `49340dcf8a92884fb95006b2ec095381` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromEast -> Connector_South_Out (gauche) |
| Intersection_Center_Crossroads | P | 0.91 | `448d024c97cec79a916b160742727798` | Junction_FromNorth -> Connector_East_Out (gauche) | Junction_FromSouth -> Connector_West_Out (gauche) |
| Intersection_Center_Crossroads | P | 0.91 | `4c3054544ab019cafa18a48c279614b0` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromWest -> Connector_North_Out (gauche) |
| Intersection_Center_Crossroads | P | 0.00 | `46987262dd6fe1111a00229ab6c9d8ab` | Junction_FromNorth -> Connector_South_Out (tout droit) | Junction_FromWest -> Connector_East_Out (tout droit) |
| Intersection_Center_Crossroads | P | 0.00 | `49c2d7c1095d014dcd2e9707187a5d9e` | Junction_FromSouth -> Connector_North_Out (tout droit) | Junction_FromEast -> Connector_West_Out (tout droit) |
| Intersection_Center_Crossroads | P | 0.00 | `45f30575f87cd3d65d78177ee3c66d85` | Junction_FromSouth -> Connector_North_Out (tout droit) | Junction_FromWest -> Connector_East_Out (tout droit) |
| Intersection_Center_Crossroads | P | 0.00 | `4fade05dfa8725d21db5702002aa4ca3` | Junction_FromNorth -> Connector_South_Out (tout droit) | Junction_FromEast -> Connector_West_Out (tout droit) |
| Intersection_Center_Crossroads | P | 0.00 | `4065d99bf14d8ee4c3e42099de20fa89` | Junction_FromWest -> Connector_South_Out (droite) | Junction_FromEast -> Connector_South_Out (gauche) |
| Intersection_Center_Crossroads | P | 0.00 | `421fede006aa2f42e5a6fd0002a6d696` | Junction_FromNorth -> Connector_East_Out (gauche) | Junction_FromSouth -> Connector_East_Out (droite) |
| Intersection_Center_Crossroads | P | 0.00 | `44ef4e39ca0ade7338fc8feec0f429be` | Junction_FromNorth -> Connector_West_Out (droite) | Junction_FromSouth -> Connector_West_Out (gauche) |
| Intersection_Center_Crossroads | P | 0.00 | `45a0180046cf5cfca98979d7fcd8828a` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromNorth -> Connector_West_Out (droite) |
| Intersection_Center_Crossroads | P | 0.00 | `47c31dcb178dc8692f142c61851d9cb7` | Junction_FromNorth -> Connector_East_Out (gauche) | Junction_FromWest -> Connector_East_Out (tout droit) |
| Intersection_Center_Crossroads | P | 0.00 | `488452c23f729a6d1d1f9aa582c2bf8f` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromSouth -> Connector_West_Out (gauche) |
| Intersection_Center_Crossroads | P | 0.00 | `4987ba61c57809e24605d2648053e29e` | Junction_FromSouth -> Connector_North_Out (tout droit) | Junction_FromWest -> Connector_North_Out (gauche) |
| Intersection_Center_Crossroads | P | 0.00 | `49a2893231955267ed28fd25ded38ba0` | Junction_FromSouth -> Connector_North_Out (tout droit) | Junction_FromEast -> Connector_North_Out (droite) |
| Intersection_Center_Crossroads | P | 0.00 | `4aeb5f7d500459bab9a69a74b6996994` | Junction_FromNorth -> Connector_South_Out (tout droit) | Junction_FromEast -> Connector_South_Out (gauche) |
| Intersection_Center_Crossroads | P | 0.00 | `4c20416c36051baa4edfb294cd6e169b` | Junction_FromWest -> Connector_North_Out (gauche) | Junction_FromEast -> Connector_North_Out (droite) |
| Intersection_Center_Crossroads | P | 0.00 | `4c31ad754ca8b25911fbb0f12ca04f90` | Junction_FromNorth -> Connector_South_Out (tout droit) | Junction_FromWest -> Connector_South_Out (droite) |
| Intersection_Center_Crossroads | P | 0.00 | `4cb4ca66e2b91ace8ba4ce3150bf3ebb` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromWest -> Connector_East_Out (tout droit) |
| Roundabout_NorthEast | P | 0.00 | `40e3e192ec85c0457be62034d55155a4` | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| Roundabout_NorthEast | P | 0.00 | `41ddaaa21dd44070074b0aa5e4581880` | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Connector_West_In -> Ring_Merge_West (entree d'anneau) |
| Roundabout_NorthEast | P | 0.00 | `4ef00a041311cff1569a33b5a476aa88` | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) |
| Roundabout_NorthWest | P | 0.00 | `405ba721192ad202e99c9ba49b5e9fb7` | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| Roundabout_NorthWest | P | 0.00 | `40b126f3a984fa21883e88b3f099319c` | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) |
| Roundabout_NorthWest | P | 0.00 | `43a738a3e1cbfb568fadc5b3548a1b8f` | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Connector_West_In -> Ring_Merge_West (entree d'anneau) |
| Roundabout_SouthEast | P | 0.00 | `45347e3ef39bcc64fdfccaf89295a4bd` | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) |
| Roundabout_SouthEast | P | 0.00 | `4e24e297c4d1bd9139c77a07f6177abb` | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Connector_West_In -> Ring_Merge_West (entree d'anneau) |
| Roundabout_SouthEast | P | 0.00 | `4e2ebb271a55553f65fe7bc0087b7482` | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| Roundabout_SouthWest | P | 0.00 | `451483d7154f55215bd3214ab24e3d98` | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) |
| Roundabout_SouthWest | P | 0.00 | `49b9fb8a95b566317bd90511c5724b82` | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) |
| Roundabout_SouthWest | P | 0.00 | `4df53fca19b7ee5211d4e512b3a4da86` | Connector_West_In -> Ring_Merge_West (entree d'anneau) | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) |
| TJunction_East | C | 0.03 | `4a435f4e82a6a473eee3d534a3378fb9` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_East | C | 0.03 | `43bbfeff3c02c3bf890cd95139e295a3` | Junction_FromWest -> Connector_East_Out (tout droit) | Junction_FromEast -> Connector_South_Out (gauche) |
| TJunction_East | C | 0.02 | `468af52ce1bbea22b1482f15af632390` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromEast -> Connector_South_Out (gauche) |
| TJunction_East | P | 0.00 | `433b2a4fbf3fe4c8557ecd7b832e4fb1` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromEast -> Connector_West_Out (tout droit) |
| TJunction_East | P | 0.00 | `47c7fee4039145473209b7b5d87472b3` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_East | P | 0.00 | `48dacd01988ac15ff1cf37499497f885` | Junction_FromWest -> Connector_South_Out (droite) | Junction_FromEast -> Connector_South_Out (gauche) |
| TJunction_North | C | 0.03 | `47d2bcbf20f128f9b2cf7d5f8dee60b9` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_North | C | 0.03 | `4e982b0fa19ae9f5ee3c0bc424e8ac91` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_North | C | 0.02 | `4782b2025438431bd3b4c6d8fedfe99b` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromSouth -> Connector_West_Out (gauche) |
| TJunction_North | P | 0.00 | `4875672f99ccdb811ea74f51b86d298f` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromSouth -> Connector_West_Out (gauche) |
| TJunction_North | P | 0.00 | `4b91ce1faf5f47a051923e55db19d084` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromWest -> Connector_South_Out (droite) |
| TJunction_North | P | 0.00 | `4e90907066432b37ea55bf8b6c7967b3` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_South | C | 0.03 | `46fef049de32bffeccdcc752588447a5` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_South | C | 0.03 | `4651661907bde7ddf496e0b38b1a67b2` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_South | C | 0.02 | `46c0852ee863ff838d59392162782297` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromEast -> Connector_South_Out (gauche) |
| TJunction_South | P | 0.00 | `4a44f9710a830e7f77c6952d114c4fbf` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromWest -> Connector_South_Out (droite) |
| TJunction_South | P | 0.00 | `4c147a4d6d6d6cd2fde23d045167c692` | Junction_FromSouth -> Connector_East_Out (droite) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_South | P | 0.00 | `4fce84fd1a162287b7b06ed08526d094` | Junction_FromSouth -> Connector_West_Out (gauche) | Junction_FromEast -> Connector_West_Out (tout droit) |
| TJunction_West | C | 0.03 | `406690a88e36b4596ca05b5832a555a9` | Junction_FromWest -> Connector_East_Out (tout droit) | Junction_FromSouth -> Connector_West_Out (gauche) |
| TJunction_West | C | 0.03 | `40ce606892963a8943f86c2ecd963ba2` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromWest -> Connector_East_Out (tout droit) |
| TJunction_West | C | 0.02 | `4b786539f4809cd080c6de032000a3bd` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromSouth -> Connector_West_Out (gauche) |
| TJunction_West | P | 0.00 | `4258af5419bba1365a3f0ad6ed3d44aa` | Junction_FromEast -> Connector_West_Out (tout droit) | Junction_FromSouth -> Connector_West_Out (gauche) |
| TJunction_West | P | 0.00 | `470bc18b1824ae1e7e47ac80c41aca9c` | Junction_FromEast -> Connector_South_Out (gauche) | Junction_FromWest -> Connector_South_Out (droite) |
| TJunction_West | P | 0.00 | `4fbcc8e5f354d981b2a3580b4645f6b7` | Junction_FromWest -> Connector_East_Out (tout droit) | Junction_FromSouth -> Connector_East_Out (droite) |
