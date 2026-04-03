<template>
  <q-page class="bg-grey-2 q-pa-md">
    <div class="row q-col-gutter-md full-height-container">
      <div class="col-12 col-md-8">
        <q-card class="game-board-card shadow-5 column flex-center">
          <div class="text-h5 q-mb-md text-weight-bold text-primary">
            三方对弈 - 房号: {{ roomCode }}
          </div>

          <div class="canvas-wrapper bg-orange-2 shadow-2 full-width flex flex-center">
            <canvas
              ref="gomokuCanvas"
              style="cursor: crosshair"
              @click="handleCanvasClick"
            ></canvas>
          </div>

          <div class="q-mt-md text-grey-7">
            提示：点击交叉点落子。当前回合：
            <q-badge :color="getPlayerColor(currentPlayer)"> 玩家 {{ currentPlayer }} </q-badge>
            (你是玩家 {{ myOrder }})
          </div>
        </q-card>
      </div>

      <div class="col-12 col-md-4 column q-gutter-y-md">
        <q-card class="q-pa-md shadow-5">
          <div class="text-subtitle1 text-bold q-mb-sm">玩家列表 ({{ playerList.length }}/3)</div>
          <q-list separator>
            <q-item v-for="player in playerList" :key="player.order">
              <q-item-section avatar>
                <q-avatar :color="getPlayerColor(player.order)" text-color="white">
                  {{ player.order }}
                </q-avatar>
              </q-item-section>

              <q-item-section>
                <q-item-label>
                  {{ player.playerName }}
                  {{ player.order === myOrder ? '(你)' : '' }}
                </q-item-label>
                <q-item-label caption>状态：已就绪</q-item-label>
              </q-item-section>
            </q-item>
          </q-list>
        </q-card>

        <q-card class="col column q-pa-md shadow-5">
          <div class="text-subtitle1 text-bold q-mb-sm">实时聊天</div>

          <q-scroll-area class="col q-mb-md bg-white rounded-borders border-grey">
            <q-list separator>
              <q-item v-for="(msg, index) in messages" :key="index" dense>
                <q-item-section>
                  <q-item-label class="text-caption">
                    <span class="text-bold text-primary">{{ msg.user }}</span>
                    <span v-if="msg.time" class="q-ml-sm text-grey-6">{{ msg.time }}</span>
                  </q-item-label>
                  <q-item-label class="text-body2">{{ msg.text }}</q-item-label>
                </q-item-section>
              </q-item>
            </q-list>
          </q-scroll-area>

          <q-input
            v-model="newMsg"
            outlined
            dense
            placeholder="按回车发送消息..."
            @keyup.enter="sendChat"
          >
            <template v-slot:after>
              <q-btn round dense flat icon="send" color="primary" @click="sendChat" />
            </template>
          </q-input>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useRoute } from 'vue-router';
import * as signalR from '@microsoft/signalr';
import { Dialog, Notify, useQuasar } from 'quasar';

const $q = useQuasar();

const route = useRoute();
const roomCode = ref(String(route.query.roomCode || ''));
const myName = ref(String(route.query.name || '未知玩家'));

// Canvas 相关引用
const gomokuCanvas = ref<HTMLCanvasElement | null>(null);
const boardSize = 15; // 15x15
const padding = 30; // 棋盘边缘留白

// 模拟数据
const messages = ref<ChatMessage[]>([{ user: '系统', text: '欢迎来到三人棋局！' }]);

const newMsg = ref('');
const newMessage = ref('');

interface ChatMessage {
  user: string;
  text: string;
  time?: string; // 加个问号表示这个字段是可选的
}

// 获取玩家代表色
const getPlayerColor = (order: number) => {
  const colors = ['black', 'grey-4', 'red']; // 黑、白、红
  return colors[order - 1];
};

const sendChat = async () => {
  // 对应 template 里的 v-model="newMsg"
  if (!newMsg.value.trim()) return;

  if (connection && connection.state === signalR.HubConnectionState.Connected) {
    try {
      // 调用后端 Hub 的 SendMessage 方法
      await connection.invoke('SendMessage', roomCode.value, myName.value, newMsg.value);
      newMsg.value = ''; // 发送成功后清空输入框
    } catch (err) {
      console.error('发送失败:', err);
    }
  }
};

// 0: 空, 1: 黑(玩家1), 2: 白(玩家2), 3: 红(玩家3)
const boardState = ref<number[][]>(Array.from({ length: 15 }, () => Array(15).fill(0)));
const currentPlayer = ref(1); // 从玩家1开始

const myOrder = ref(0); // 记录我是玩家 1, 2 还是 3

interface PlayerInfo {
  playerName: string;
  order: number;
  color: string;
}

// 定义一个落子记录的接口，替代 any
interface GameMove {
  row: number;
  col: number;
  playerOrder: number;
}

// 定义后端传回来的完整数据结构
interface BoardStateData {
  moves: GameMove[];
  currentTurn: number;
}

const playerList = ref<PlayerInfo[]>([]);

// --- SignalR 相关 ---
let connection: signalR.HubConnection | null = null;

/********************************** SignalR Start **********************************/
const initSignalR = async () => {
  if (connection) {
    await connection.stop();
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7239/gomokuHub')
    .withAutomaticReconnect()
    .build();

  // --- 1. 先把所有的【频道】(监听器) 全部挂上去 ---

  // --- 监听落子 ---
  connection.on('StonePlaced', (row: number, col: number, player: number, nextTurn: number) => {
    console.log(`收到落子同步：玩家 ${player} 在 ${row},${col}`);

    const rowData = boardState.value[row];
    if (rowData) {
      rowData[col] = player;
      refreshCanvas();

      // 检查胜负
      if (checkWin(row, col, player)) {
        // 使用大写的 Dialog 关键字直接调用
        Dialog.create({
          title: '游戏结束',
          message: `恭喜玩家 ${player} 获胜！`,
          persistent: true,
          ok: {
            label: '回到主页',
            color: 'primary',
          },
        }).onOk(() => {
          window.location.hash = '/';
        });
        return;
      }

      // 使用后端传回来的 nextTurn，确保所有人步调一致
      currentPlayer.value = nextTurn;
    }
  });

  // --- 监听加入成功 (给自己赋值) ---
  connection.on('JoinSuccess', (data: { order: number; color: string }) => {
    console.log('收到加入成功:', data);
    myOrder.value = data.order;
    $q.notify({ message: `入场成功！你是玩家 ${data.order}`, color: 'positive' });
  });

  // --- 监听玩家加入广播 ---
  connection.on('PlayerJoined', (data: PlayerInfo) => {
    if (!playerList.value.find((p) => p.order === data.order)) {
      playerList.value.push(data);
    }
    messages.value.push({ user: '系统', text: `${data.playerName} 进入了房间` });
  });

  // --- 监听消息广播 (所有人可见) ---
  connection.on('ReceiveMessage', (user: string, text: string) => {
    messages.value.push({
      user: user,
      text: text,
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    });
  });

  // --- 监听列表更新 ---
  connection.on('UpdatePlayerList', (players: PlayerInfo[]) => {
    playerList.value = players;
  });

  // --- 监听棋盘更新 ---
  connection.on('UpdateBoardState', (data: BoardStateData) => {
    console.log('正在同步棋盘状态...', data);

    // 1. 同步当前回合
    currentPlayer.value = data.currentTurn;

    // 2. 填充棋盘数组
    // 重置棋盘
    boardState.value = Array.from({ length: 15 }, () => Array(15).fill(0));

    data.moves.forEach((move) => {
      // 解决方法：先确保行存在，再赋值
      const row = boardState.value[move.row];
      if (row !== undefined) {
        row[move.col] = move.playerOrder;
      }
    });

    // 3. 核心：重绘 Canvas 画面
    refreshCanvas();
  });

  connection.on('ReceiveChatHistory', (history: ChatMessage[]) => {
    console.log('加载历史记录:', history);

    // 保留那个初始的“系统欢迎语”，然后把历史记录接在后面
    // 用解构赋值 [...] 可以触发 Vue 的响应式更新
    messages.value = [{ user: '系统', text: '欢迎来到三人棋局！' }, ...history];
  });

  connection.on('ReceiveError', (error: string) => {
    $q.notify({ type: 'negative', message: error, position: 'top', timeout: 2500 });
  });

  try {
    await connection.start();
    console.log('SignalR 连接成功，开始初始化数据...');

    // 4. 连接成功后，发送请求
    await connection.invoke('JoinRoom', roomCode.value, myName.value);
    await connection.invoke('GetRoomPlayers', roomCode.value);
    await connection.invoke('GetBoardState', roomCode.value);
    await connection.invoke('GetChatHistory', roomCode.value);
  } catch (err) {
    console.error('SignalR 启动失败:', err);
  }
};
/********************************** SignalR End **********************************/

/********************************** 绘制棋盘 Start **********************************/
onMounted(() => {
  console.log('当前本地记录的名字是:', myName.value); // 确认这里不是空的或者默认值
  window.addEventListener('resize', initCanvas);
  initCanvas();
  void initSignalR();
});

onUnmounted(() => {
  if (connection) {
    void connection.stop(); // 加 void
  }
  window.removeEventListener('resize', initCanvas);
});

const initCanvas = () => {
  const canvas = gomokuCanvas.value;
  if (!canvas) return;

  const ctx = canvas.getContext('2d');
  if (!ctx) return;

  // 1. 获取父容器
  const parent = canvas.parentElement;
  if (!parent) return;

  // 2. 同时获取父容器的宽度和高度
  const availableWidth = parent.clientWidth;
  // 我们往上找 game-board-card 的实际渲染高度，减去标题和文字占用的空间（约150px）
  const boardCard = canvas.closest('.game-board-card');
  const availableHeight = (boardCard?.clientHeight || 600) - 150;

  // 3. 取宽和高中较小的一个作为边长，确保它是正方形
  // 我们把减去的边距缩小（比如只减 20），让棋盘尽可能铺满
  const size = Math.min(availableWidth, availableHeight) - 20;

  canvas.width = size;
  canvas.height = size;

  drawBoard(ctx, size);
};

const drawBoard = (ctx: CanvasRenderingContext2D, size: number) => {
  const cellSize = (size - padding * 2) / (boardSize - 1); // 计算格子间距

  ctx.clearRect(0, 0, size, size); // 清空画布

  // 1. 设置线条样式
  ctx.strokeStyle = '#333';
  ctx.lineWidth = 1;

  // 2. 画横线和纵线
  for (let i = 0; i < boardSize; i++) {
    const pos = padding + i * cellSize;

    // 横线
    ctx.beginPath();
    ctx.moveTo(padding, pos);
    ctx.lineTo(size - padding, pos);
    ctx.stroke();

    // 纵线
    ctx.beginPath();
    ctx.moveTo(pos, padding);
    ctx.lineTo(pos, size - padding);
    ctx.stroke();
  }

  // 3. 画五个“星位”（五子棋盘上的小黑点）
  const stars = [3, 7, 11]; // 对应的索引
  stars.forEach((row) => {
    stars.forEach((col) => {
      // 只有四角和中间画圆点
      if ((row === 7 && col === 7) || (row !== 7 && col !== 7)) {
        drawStar(ctx, padding + col * cellSize, padding + row * cellSize);
      }
    });
  });
};

const drawStar = (ctx: CanvasRenderingContext2D, x: number, y: number) => {
  ctx.beginPath();
  ctx.arc(x, y, 4, 0, Math.PI * 2);
  ctx.fillStyle = '#000';
  ctx.fill();
};
/********************************** 绘制棋盘 End **********************************/

/********************************** 下棋逻辑 Start **********************************/
const handleCanvasClick = (event: MouseEvent) => {
  // --- 回合限制 ---
  if (currentPlayer.value !== myOrder.value) {
    $q.notify({ type: 'warning', message: '还没轮到你！', position: 'top' });
    return;
  }

  const canvas = gomokuCanvas.value;
  if (!canvas) return;

  const rect = canvas.getBoundingClientRect();
  // 计算相对于 canvas 左上角的像素坐标
  const x = event.clientX - rect.left;
  const y = event.clientY - rect.top;

  // 计算格子大小（必须和 drawBoard 里的逻辑一致）
  const size = canvas.width;
  const cellSize = (size - padding * 2) / (boardSize - 1);

  // 1. 估算点击的是哪一行哪一列
  const col = Math.round((x - padding) / cellSize);
  const row = Math.round((y - padding) / cellSize);

  // 2. 边界检查：必须在 0-14 范围内
  if (row >= 0 && row < boardSize && col >= 0 && col < boardSize) {
    // 3. 判定点击点是否足够靠近交叉点 (半径允许误差范围，比如 cellSize 的 40%)
    const targetX = padding + col * cellSize;
    const targetY = padding + row * cellSize;
    const distance = Math.sqrt(Math.pow(x - targetX, 2) + Math.pow(y - targetY, 2));

    if (distance < cellSize * 0.4) {
      void placeStone(row, col);
    }
  }
};

const placeStone = async (row: number, col: number) => {
  const rowData = boardState.value[row];
  if (!rowData || rowData[col] !== 0) return;

  if (connection && connection.state === signalR.HubConnectionState.Connected) {
    try {
      // 调用后端的 PlaceStone (注意首字母大小写和参数顺序)
      // 参数：roomCode, x, y, playerOrder
      await connection.invoke('PlaceStone', roomCode.value, row, col, myOrder.value);
    } catch (err) {
      console.error('落子失败:', err);
    }
  }
};

// 新增：刷新画布，先画线，再画棋子
const refreshCanvas = () => {
  const canvas = gomokuCanvas.value;
  if (!canvas) return;
  const ctx = canvas.getContext('2d');
  if (!ctx) return;

  drawBoard(ctx, canvas.width);

  const cellSize = (canvas.width - padding * 2) / (boardSize - 1);

  for (let r = 0; r < boardSize; r++) {
    const rowData = boardState.value[r];
    if (!rowData) continue; // 类型保护：如果这一行不存在就跳过

    for (let c = 0; c < boardSize; c++) {
      const type = rowData[c];
      // 只要 type 是有效的数字且不为 0
      if (type && type !== 0) {
        drawPiece(ctx, padding + c * cellSize, padding + r * cellSize, type);
      }
    }
  }
};

// 画棋子的具体函数
const drawPiece = (ctx: CanvasRenderingContext2D, x: number, y: number, type: number) => {
  const canvas = gomokuCanvas.value;
  const cellSize = ((canvas?.width || 600) - padding * 2) / (boardSize - 1);

  ctx.beginPath();
  ctx.arc(x, y, cellSize * 0.4, 0, Math.PI * 2);

  // 根据玩家编号设颜色
  if (type === 1) ctx.fillStyle = '#000'; // 玩家1：黑
  if (type === 2) ctx.fillStyle = '#FFFFFF'; // 玩家2：白（这里可以加个黑边框）
  if (type === 3) ctx.fillStyle = '#FF0000'; // 玩家3：红

  ctx.fill();

  // 给棋子加个细边框，让白色棋子在淡黄背景上更清晰
  ctx.strokeStyle = '#333';
  ctx.lineWidth = 1;
  ctx.stroke();
};
/********************************** 下棋逻辑 End **********************************/

/********************************** 胜负判定算法 Start **********************************/
const checkWin = (row: number, col: number, player: number): boolean => {
  const board = boardState.value;
  // 定义四个方向
  const directions: [number, number][] = [
    [0, 1], // 水平
    [1, 0], // 垂直
    [1, 1], // 右下斜
    [1, -1], // 左下斜
  ];

  for (const dir of directions) {
    // 显式解构，这样 dr 和 dc 就不会是 undefined 了
    const [dr, dc] = dir;
    let count = 1;

    // 正向检查
    let r = row + dr;
    let c = col + dc;
    // 使用 optional chaining ?. 或者确保 rowData 存在
    while (r >= 0 && r < 15 && c >= 0 && c < 15 && Number(board[r]?.[c]) === Number(player)) {
      count++;
      r += dr;
      c += dc;
    }

    // 反向检查
    r = row - dr;
    c = col - dc;
    while (r >= 0 && r < 15 && c >= 0 && c < 15 && Number(board[r]?.[c]) === Number(player)) {
      count++;
      r -= dr;
      c -= dc;
    }

    if (count >= 5) return true;
  }
  return false;
};
/********************************** 胜负判定算法 End **********************************/
</script>

<style scoped>
.full-height-container {
  /* 增加这里的高度，让它接近全屏 */
  height: calc(100vh - 40px);
}

.game-board-card {
  height: 100%;
  /* 允许卡片内部元素垂直分布 */
  display: flex;
  flex-direction: column;
  justify-content: space-between; /* 标题在上，提示在下，棋盘居中 */
  padding: 20px;
  background-color: white;
  border-radius: 12px;
}

.canvas-wrapper {
  /* 去掉之前的 full-width，让 flex-center 决定位置 */
  padding: 15px;
  border-radius: 8px;
  border: 8px solid #5d4037; /* 加厚边框增加质感 */
  background-color: #f3e5ab; /* 木质底色 */
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);

  /* 确保在 flex 容器中它是自适应的 */
  display: flex;
  align-items: center;
  justify-content: center;
}

.border-grey {
  border: 1px solid #e0e0e0;
}
</style>
