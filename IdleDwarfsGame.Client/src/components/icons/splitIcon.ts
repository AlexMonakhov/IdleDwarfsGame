// Пропсы иконок на SplitIcon: те же, что у IconHelm. Цвета не заданы — берутся у родителя
// через CSS-переменные --split-icon-primary / --split-icon-secondary, а без них — цвета шлема в игре
export interface SplitIconProps {
  size?: string;
  primaryColor?: string;
  secondaryColor?: string;
}
