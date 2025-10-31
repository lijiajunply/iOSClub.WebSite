<template>
  <div :style="style">
    <svg class="icon" aria-hidden="true">
      <use :href="type"></use>
    </svg>
  </div>
</template>

<script setup>
defineProps({
  type: {
    type: String,
    required: true
  },
  style: String
})
</script>