import Phaser from 'phaser';
import { BootScene } from './scenes/BootScene';
import { TitleScene } from './scenes/TitleScene';
import { StarterSelectScene } from './scenes/StarterSelectScene';
import { OverworldScene } from './scenes/OverworldScene';
import { BattleScene } from './scenes/BattleScene';
import { DinoCenterScene } from './scenes/DinoCenterScene';
import { ShopScene } from './scenes/ShopScene';
import { PartyScene } from './scenes/PartyScene';
import { BagScene } from './scenes/BagScene';
import { DinodexScene } from './scenes/DinodexScene';
import { EvolutionScene } from './scenes/EvolutionScene';
import { GymScene } from './scenes/GymScene';
import { EliteFourScene } from './scenes/EliteFourScene';
import { HouseScene } from './scenes/HouseScene';
import { GAME_WIDTH, GAME_HEIGHT } from './utils/constants';

const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  width: GAME_WIDTH,
  height: GAME_HEIGHT,
  parent: document.body,
  pixelArt: true,
  roundPixels: true,
  scale: {
    mode: Phaser.Scale.FIT,
    autoCenter: Phaser.Scale.CENTER_BOTH,
  },
  dom: {
    createContainer: true,
  },
  input: {
    keyboard: true,
  },
  physics: {
    default: 'arcade',
    arcade: {
      gravity: { x: 0, y: 0 },
      debug: false,
    },
  },
  fps: {
    target: 60,
    forceSetTimeOut: false,
  },
  backgroundColor: '#181018',
  scene: [
    BootScene,
    TitleScene,
    StarterSelectScene,
    OverworldScene,
    BattleScene,
    DinoCenterScene,
    ShopScene,
    PartyScene,
    BagScene,
    DinodexScene,
    EvolutionScene,
    GymScene,
    EliteFourScene,
    HouseScene,
  ],
};

const game = new Phaser.Game(config);
(window as any).__PHASER_GAME__ = game;

export default game;
