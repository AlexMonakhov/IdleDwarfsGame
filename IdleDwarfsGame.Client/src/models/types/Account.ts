// Эффекты рамки аватара. Названия и условия — в i18n: account.frames.<key>
export const FRAME_EFFECTS = ['none', 'snow', 'embers', 'runes'] as const;
export type FrameEffect = typeof FRAME_EFFECTS[number];

export interface FrameEffectInfo {
    key: FrameEffect;
    unlocked: boolean;
}
