<template>
  <q-page class="index-bg flex flex-center">
    <q-card class="glass-card q-pa-xl shadow-10 text-center">
      <q-card-section>
        <div class="text-h2 text-weight-bolder text-white text-shadow q-mb-xl">GomokuGame</div>
      </q-card-section>

      <q-card-section class="column q-gutter-y-lg">
        <q-input
          v-model="playerName"
          label="您的昵称"
          dark
          color="deep-orange"
          filled
          class="full-width"
        />

        <q-btn
          style="background: #6600cc; color: white"
          glossy
          label="创建新房间"
          size="lg"
          class="full-width"
          @click="createRoom"
        />

        <div class="row items-center q-my-sm">
          <q-separator class="col" dark />
          <span class="q-px-sm text-white">OR</span>
          <q-separator class="col" dark />
        </div>

        <div class="column q-gutter-y-sm">
          <q-input
            v-model="inputRoomCode"
            label="输入6位房号"
            dark
            color="deep-orange"
            outlined
            mask="XXXXXX"
            class="full-width"
          >
            <template v-slot:append>
              <q-icon name="meeting_room" color="white" />
            </template>
          </q-input>

          <q-btn
            outline
            color="white"
            label="加入现有房间"
            size="lg"
            class="full-width"
            @click="joinRoom"
          />
        </div>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import axios from 'axios';
import { api } from 'boot/axios';

const $q = useQuasar();
const router = useRouter();
const playerName = ref('Player_' + Math.floor(Math.random() * 100));
const inputRoomCode = ref('');

const createRoom = async () => {
  try {
    // 直接使用 api.post，路径只需要写后面的部分
    const res = await api.post('/api/games/create');
    const { roomCode } = res.data;

    $q.notify({ type: 'positive', message: '房间创建成功！' });

    await router.push({
      name: 'game',
      query: { roomCode, name: playerName.value },
    });
  } catch (e) {
    console.error(e);
    $q.notify({
      type: 'negative',
      message: '连接服务器失败，请检查服务器端口',
      position: 'top',
      timeout: 2500,
    });
  }
};

const joinRoom = async () => {
  if (inputRoomCode.value.length === 6) {
    await router.push({
      name: 'game',
      query: { roomCode: inputRoomCode.value.toUpperCase(), name: playerName.value },
    });
  } else {
    $q.notify({
      type: 'warning',
      message: '请输入完整的6位房号',
      position: 'top',
      timeout: 2500,
    });
  }
};
</script>

<style scoped>
/* 背景图设置 */
.index-bg {
  background-image: url('../assets/wallpaper.jpg'); /* 在这里替换你的图片地址 */
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  min-height: 100vh;
}

/* 玻璃拟态卡片效果 */
.glass-card {
  background: rgba(102, 102, 255, 0.75); /* 黑色半透明背景 */
  backdrop-filter: blur(8px); /* 背景模糊效果 */
  border-radius: 20px;
  border: 1px solid rgba(255, 255, 255, 0.2);
  width: 90%;
  max-width: 500px; /* 限制最大宽度，但在手机端会自动缩放 */
}

/* 文字阴影，让白色大字更清晰 */
.text-shadow {
  text-shadow: 2px 2px 10px rgba(0, 0, 0, 0.8);
}
</style>
