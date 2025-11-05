<template>
  <div :style="style">
    <svg :class="className" aria-hidden="true">
      <use :href="str"></use>
    </svg>
  </div>
</template>

<script setup>
import {computed} from "vue";

const props = defineProps({
  type: {
    type: String,
    required: true
  },
  style: String,
  className: String,
})

const str = computed(() => {
  return `#icon-${props.type}`
})
</script>