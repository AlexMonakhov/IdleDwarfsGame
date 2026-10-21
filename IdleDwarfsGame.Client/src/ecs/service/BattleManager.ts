import { updateHeroHp } from "@/models/composables/heroes";
import { createEntity } from "../entities/EntityFactory"
import { Entity } from "../entities/Storage";
import { animationSystem } from "../systems/AnimationSystem";
import { battleSystem } from "../systems/BattleSystem";
import { reactive } from "vue";
class BattleManager {

    constructor(config: BattleManagerConfig) {
        this.updateHeroHp = config.updateHeroHp;
        this.configureSystems();
    }

    private updateHeroHp: (entity: number, newHp: number) => void;
    private teamA: Entity[] = [];
    private teamB: Entity[] = [];
    
    state = reactive({
      round : 1,
      maxRound : 50,
      winner : ''
    })
    
    skip = false;
    private isBattleRunning = false;
  
    createEntities() {
      const hero = createEntity('Герой', 100, 30, 13);
      const hero1 = createEntity('Герой', 100, 30, 10);
      const hero2 = createEntity('Герой', 100, 30, 10);
      const hero3 = createEntity('Герой', 100, 30, 10);
      const hero4 = createEntity('Герой', 100, 30, 10);
      const goblin = createEntity('Гоблин', 60, 100, 12);
      const goblin1 = createEntity('Гоблин', 60, 15, 12);
      const goblin2 = createEntity('Гоблин', 60, 15, 12);
      const orc = createEntity('Орк', 50, 25, 8);
      const orc1 = createEntity('Орк', 50, 25, 8);

  
      this.teamA = [hero, hero1, hero2, hero3,hero4];
      this.teamB = [goblin, goblin1, goblin2, orc, orc1];
    }

    getTeams(): [Entity[], Entity[]] {
      return [this.teamA, this.teamB]
    }
  
    async StartBattle(): Promise<void> {
      if (this.isBattleRunning) return;

      this.isBattleRunning = true;
      while (this.continueFight()) {
        console.log(`Раунд ${this.state.round} начался⚔️!`);
        this.state.winner = battleSystem.execute(this.teamA, this.teamB);
        
        await animationSystem.execute(() => this.skip);
        this.nextRound();
      }
  
      console.log(this.state.winner ? `🏆 Победила ${this.state.winner}` : '🛑 Ничья');
      this.isBattleRunning = false;
    }
  
    nextRound() {
      this.state.round++;
    }
  
    continueFight() {
      return this.state.round <= this.state.maxRound && !this.state.winner;
    }
  
    skipFight() {
      this.skip = true;
    }

    configureSystems() {
        animationSystem.init(this.updateHeroHp);
    }
  }

  export interface BattleManagerConfig {
    updateHeroHp: (entity: Entity, newHp: number) => void;
  }

  export const battleManager = new BattleManager({ updateHeroHp });

