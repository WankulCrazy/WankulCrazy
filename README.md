<!-- ABOUT THE PROJECT -->
## À propos du projet

Le projet a été initié par une demande de nos streamers préférés, ils souhaitaient pouvoir intégrer leur univers au jeu TCG Card Shop Simulator.<br>
C'est dans cet optique que nous avons développé ce mod. Celui-ci permet de remplacer les textures des assets en jeu et de rajouter les cartes Wankuls ainsi que leur fonctionnement.<br>
Lorsque nous avons débuté le projet, nous aurions apprécié trouver des projets similaires pour nous inspirer dans notre développement, c'est pourquoi étant passionnés de développement / programmation, nous avons décidé de rendre les sources du mod publiques.<br>
Toute fois, nous n'autorisons pas le copier / coller du code cf: LICENCE.txt, nous favorisons la recherche et la réflexion.<br>
Nous rappelons que le projet n'a pas pour but d'être commercialisé, et que nous ne demandons aucune forme de financement.<br>
L'utilisation des textures de cartes Wankuls pour tout autre projet doit avant tout être validé, et autorisé par Wankil.


### Généré avec

Le mod a été généré avec :

* BepInEx (5.4.23.2): https://github.com/BepInEx/BepInEx
* Harmony (2.7.0): https://harmony.pardeike.net/
* Unity (v2021.3.38.8007589): https://unity.com/


<!-- GETTING STARTED -->
## Pour générer

Pour générer le projet il vous faudra suivre ces étapes.

### Prérequis

* Visual Studio 2022:
* BepInEx (5.4.23.2) d'installé dans le dossier du jeu

### Installation


1. Cloner le repo<br>
   ```
   git clone https://github.com/WankulCrazy/WankulCrazy.git
   ```
2. Télécharger les textures<br>
   Lancez le script suivant dans powershell
   ```
   downloadAssets.ps1
   ```
3. Générer<br>
   Lancez la génération du projet depuis visual studio avec le preset ReleaseNoDeps
4. Lancer le mod<br>
   Copiez le contenu du dossier dist généré dans le dossier plugins de BepInEx.


<!-- CONTRIBUTING -->
## Contribuer

Pour contribuer, je vous invite à forker le projet et à créer une pull request avec vos modifications.
Si vous avez des questions, n'hésitez pas à contacter @ToHo sur les discords Wankil, de préférence engagez la conversation en MP pour ne pas flood le serveur.


<!-- LICENSE -->
## License

WankulCrazy © 2024 by ToHold is licensed under Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International. To view a copy of this license, visit https://creativecommons.org/licenses/by-nc-nd/4.0/

All cards texture belongs to wankul.fr, we are not owner of this game card, just allowed to use their card textures.
Any use of these cards needs to be allowed from Wankil © 2024.

Tanks for contributing :
- Karilla for helping on code
- Hurtem for retexturing assets 
