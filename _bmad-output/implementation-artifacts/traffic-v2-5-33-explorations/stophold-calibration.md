# Calibration du maintien a l'arret D11 (Story 5.33)

Banc point-masse, arbitrage seul, leader synthetique ; s0 2 m, vitesse d'entree et de depart 0.5 m/s (VehicleTireModel.SlipReferenceSpeed). Physique : acceleration commandee, roue libre a 2 m/s2 sous la bande de service 0.41 m/s (mesure PlayMode). Approche depuis 40 m d'un leader arrete ; depart du leader a 30 s.

Rampement : distance sous la vitesse d'entree, acceleration > 0 liee a une interaction, hors maintien, source arretee.

| Parametres | v0 (m/s) | jeu a l'entree | jeu maintenu | jeu min | rampement (m) | liberation au depart | delai (s) | re-entrees apres depart |
|---|---|---|---|---|---|---|---|---|
| IDM seul (maintien desactive) | 1 | - | 2 | 2 | 0.395 | None | - | 0 |
| IDM seul (maintien desactive) | 2 | - | 2 | 2 | 0.394 | None | - | 0 |
| IDM seul (maintien desactive) | 4 | - | 2 | 2 | 0.397 | None | - | 0 |
| IDM seul (maintien desactive) | 6 | - | 2 | 2 | 0.394 | None | - | 0 |
| IDM seul (maintien desactive) | 8 | - | 2 | 2 | 0.398 | None | - | 0 |
| Delta_hold 0.25 / Delta_release 1 | 1 | 2.247 | 2.243 | 2.243 | 0.157 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 1 | 2 | 2.25 | 2.245 | 2.245 | 0.153 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 1 | 4 | 2.248 | 2.244 | 2.244 | 0.158 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 1 | 6 | 2.249 | 2.245 | 2.245 | 0.153 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 1 | 8 | 2.248 | 2.244 | 2.244 | 0.158 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 1.5 | 1 | 2.247 | 2.243 | 2.243 | 0.157 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 1.5 | 2 | 2.25 | 2.245 | 2.245 | 0.153 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 1.5 | 4 | 2.248 | 2.244 | 2.244 | 0.158 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 1.5 | 6 | 2.249 | 2.245 | 2.245 | 0.153 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 1.5 | 8 | 2.248 | 2.244 | 2.244 | 0.158 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 2 | 1 | 2.247 | 2.243 | 2.243 | 0.157 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 2 | 2 | 2.25 | 2.245 | 2.245 | 0.153 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 2 | 4 | 2.248 | 2.244 | 2.244 | 0.158 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 2 | 6 | 2.249 | 2.245 | 2.245 | 0.153 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.25 / Delta_release 2 | 8 | 2.248 | 2.244 | 2.244 | 0.158 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 1 | 1 | 2.498 | 2.452 | 2.452 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 1 | 2 | 2.497 | 2.451 | 2.451 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 1 | 4 | 2.492 | 2.447 | 2.447 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 1 | 6 | 2.497 | 2.451 | 2.451 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 1 | 8 | 2.493 | 2.448 | 2.448 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 1.5 | 1 | 2.498 | 2.452 | 2.452 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 1.5 | 2 | 2.497 | 2.451 | 2.451 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 1.5 | 4 | 2.492 | 2.447 | 2.447 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 1.5 | 6 | 2.497 | 2.451 | 2.451 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 1.5 | 8 | 2.493 | 2.448 | 2.448 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 2 | 1 | 2.498 | 2.452 | 2.452 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 2 | 2 | 2.497 | 2.451 | 2.451 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 2 | 4 | 2.492 | 2.447 | 2.447 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 2 | 6 | 2.497 | 2.451 | 2.451 | 0 | SourceDeparted | 0.34 | 0 |
| Delta_hold 0.5 / Delta_release 2 | 8 | 2.493 | 2.448 | 2.448 | 0 | SourceDeparted | 0.34 | 0 |

| Parametres | leader rampant 0,05 m/s pendant 60 s : liberations / rampement (m) | frisson 0,21 m : liberations | arret-depart du leader : liberations / re-entrees / rampement (m) |
|---|---|---|---|
| Delta_hold 0.25 / Delta_release 1 | 3 / 2.3 | 0 | 1 / 1 / 0.299 |
| Delta_hold 0.25 / Delta_release 1.5 | 2 / 1.111 | 0 | 1 / 1 / 0.299 |
| Delta_hold 0.25 / Delta_release 2 | 1 / 0.577 | 0 | 1 / 1 / 0.299 |
| Delta_hold 0.5 / Delta_release 1 | 4 / 1.554 | 0 | 1 / 1 / 0 |
| Delta_hold 0.5 / Delta_release 1.5 | 2 / 0.4 | 0 | 1 / 1 / 0 |
| Delta_hold 0.5 / Delta_release 2 | 1 / 0.163 | 0 | 1 / 1 / 0 |

Retenu (TrafficV2Settings.StopHold) : Delta_hold 0.5 m, Delta_release 2 m.
