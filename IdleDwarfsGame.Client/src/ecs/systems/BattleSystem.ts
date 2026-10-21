import { Entity, healths, damages, speeds, names, animations } from "../entities/Storage"
import { logService } from "../service/LogService";

class BattleSystem{

  animationOrder = 0;
  winners = [];

  execute(teamA: Entity[], teamB: Entity[]): string {
    const all = [...teamA, ...teamB];
    
    // сортировка по скорости
    const sorted = [...all].sort((a, b) => {
      return (speeds.get(b)?.value || 0) - (speeds.get(a)?.value || 0);
    });

    console.log('⚔️ Бой начинается между двумя отрядами!\n');

    for (const attacker of sorted) {
      const attackerHealth = healths.get(attacker);
      if (!attackerHealth || attackerHealth.value <= 0) continue;

      const isTeamA = teamA.includes(attacker);
      const enemies = isTeamA ? teamB : teamA;

      const aliveEnemies = enemies.filter(e => (healths.get(e)?.value || 0) > 0);
      if (aliveEnemies.length === 0) {
        const winnerTeam = isTeamA ? "Команда A" : "Команда B";
        console.log(`🏆 ${names.get(attacker)} выигрывает бой за ${winnerTeam}!`);
        return winnerTeam;
      }     

      const targets = [aliveEnemies[0]];
      this.attack(attacker, targets);
      
    }

    console.log('\n🧾 Результаты боя:');
    for (const id of all) {
      const hp = healths.get(id)?.value ?? 0;
      console.log(`${names.get(id)}: ${hp} HP`);
    }

    return '';
  }

  private attack(attacker: number, targets: number[]): void {
    const dmg = damages.get(attacker)?.value || 0;
  
    // 1. Один order для всех связанных анимаций
    const order = this.animationOrder++;
  
    // 2. Анимация атаки (всегда одна)
    if (!animations.has(attacker)) {
      animations.set(attacker, []);
    }
    animations.get(attacker)!.push({
      entity: attacker,
      type: 'attack',
      order,
      animationName: names.get(attacker)!,
    });
  
    // 3. Анимации попаданий по всем целям
    for (const target of targets) {
      const targetHealth = healths.get(target);
      if (!targetHealth) continue;
  
      targetHealth.value -= dmg;
      if (targetHealth.value < 0) targetHealth.value = 0;
  
      if (!animations.has(target)) {
        animations.set(target, []);
      }
  
      animations.get(target)!.push({
        entity: target,
        type: 'hit',
        order,
        animationName: names.get(target)!,
        newHp: targetHealth.value,
      });
  
      logService.logInfo(`${names.get(attacker)} атакует ${names.get(target)} и наносит ${dmg} урона. Осталось HP: ${targetHealth.value}`);
    }
  }
}

export const battleSystem = new BattleSystem();


