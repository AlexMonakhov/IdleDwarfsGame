import { animations, Entity, names } from "../entities/Storage";
import { Animation } from "../components/Animation";
import { getComponent } from "../entities/ComponentRegistry";
type AnimationBatch = Animation[];

class AnimationSystem {
  private updateHeroHp?: (entity: number, newHp: number) => void;

  init(updateHeroHp: (entity: Entity, newHp: number) => void) {
    this.updateHeroHp = updateHeroHp;
  }

  async execute(isSkipModeGetter: () => boolean) {
    const allAnimations = Array.from(animations.values()).flat();
    console.log("🌀 Анимаций в очереди:", animations);
    allAnimations.sort((a, b) => a.order - b.order);

    const batches = this.groupAnimationsByOrder(allAnimations);

    for (const [index, batch] of batches.entries()) {
        if (isSkipModeGetter()) {
        for (const anim of batch) {
            if (anim.type === 'hit' && anim.newHp !== undefined) {
            this.updateHeroHp?.(anim.entity, anim.newHp);
            }
        }
        continue;
        }

        if (index === 0) {
          // ⏱️ Добавим маленькую паузу перед первой группой
          await new Promise(res => setTimeout(res, 300));
        }

        await Promise.all(batch.map(anim => this.playAnimation(anim)));
    }

    animations.clear();
    if (!isSkipModeGetter()) {
      console.log("✅ Все анимации отыграли.");
    }
  }

  private async playAnimation(animation: Animation): Promise<void> {
    const handler = this.animationHandlers[animation.type];
    handler?.call(this, animation);
    return new Promise(resolve => {
      setTimeout(resolve, 1000);
    });
  }



  groupAnimationsByOrder(animations: Animation[]): AnimationBatch[] {
    const grouped: AnimationBatch[] = [];
    let currentBatch: AnimationBatch = [];
    let currentOrder: number | null = null;

    for (const anim of animations) {
        if (currentOrder === null || anim.order === currentOrder) {
        currentBatch.push(anim);
        currentOrder = anim.order;
        } else {
        grouped.push(currentBatch);
        currentBatch = [anim];
        currentOrder = anim.order;
        }
    }

    if (currentBatch.length > 0) {
        grouped.push(currentBatch);
    }

    return grouped;
  }


  // 🧩 Обработчики типов анимации

  private handleHit(animation: Animation) {
    if (animation.newHp !== undefined) {
      this.updateHeroHp?.(animation.entity, animation.newHp);
    }

    const component = getComponent(animation.entity);
    component?.exposed?.hit?.();
    console.log(`anim hit ${animation.entity}`);
  }

  private handleAttack(animation: Animation) {
    const component = getComponent(animation.entity);
    component?.exposed?.attack?.();
    console.log(`anim attack ${animation.entity}`);
  }

  // Можно добавить другие типы:
  // private handleHeal(animation: Animation) { ... }
  // private handleDodge(animation: Animation) { ... }

  private animationHandlers: Record<string, (animation: Animation) => void> = {
    hit: this.handleHit,
    attack: this.handleAttack,
    // heal: this.handleHeal,
    // dodge: this.handleDodge,
  };
}

export const animationSystem = new AnimationSystem();
