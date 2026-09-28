# Faisabilite du premier angle — TJunction_South / SE

- Baseline : commit `58c230a311ef61f81611ca640dd28b4c5bac28a9`, captures EditMode et PlayMode dans ce dossier avant toute edition Unity.
- Coupe d'essai : `c = 1,00 m` ; collider source override (`size.x = 3`, `center.x = 0,5`) + deux enfants `BoxCollider`, dont un tourne de -45° ; ancien visuel Synty desactive par override et trois cubes Unity avec `Greybox_Ground_Mat` ajoutes en visuels.
- Sauvegarde/rechargement : `MVP_Run.unity` rechargee ; scene `isDirty=false`. Les 3 colliders, les 3 renderers, la declaration `NavMeshModifier` `Sidewalk` (aire resolue par nom), le mesh cube et le materiau sont presents apres rechargement.
- Forme : grille locale a 1 cm : union des colliders = carre 4 x 4 m moins le triangle `u + v < 1 m` ; zero point divergent hors de la frontiere. Les bornes XZ collider/renderer correspondent pour les trois pieces a 1 mm. La surface declarative couvre les colliders enfants via `applyToChildren` ; l'ancien renderer Synty est inactif.
- Visuel : `first-corner.png` et `first-corner-top.png`. La coupe est visible et les trois faces grises se raccordent sans artefact de chevauchement apparent. Le gris contraste volontairement avec le trottoir Synty clair, conformement au candidat gris de la spec.
- Diff : seul `Assets/RoadRage/App/Scenes/MVP_Run.unity` a change sous `Assets/RoadRage` (409 ajouts, 1 suppression YAML) ; aucun prefab, materiau ou asset Synty modifie. La capture PNG generee par erreur sous `Assets/_bmad-output` a ete deplacee ici et ses metas/dossiers temporaires retires.
- Gate 1 : passe pour la construction candidate. Les onze autres angles ne sont pas encore deployes. La taille definitive et les residus restent a calculer.
