import { useEffect } from 'react'
import { animate, motion, useMotionValue, useReducedMotion, useTransform } from 'motion/react'
import { cn } from '@/lib/utils'

type Props = {
  value: number
  format: (v: number) => string
  className?: string
}

/** Плавно «докручивает» число до нового значения при смене периода */
export function AnimatedNumber({ value, format, className }: Props) {
  const reduceMotion = useReducedMotion()
  const motionValue = useMotionValue(reduceMotion ? value : 0)
  const text = useTransform(motionValue, (v) => format(v))

  useEffect(() => {
    const controls = animate(motionValue, value, {
      duration: reduceMotion ? 0 : 0.8,
      ease: [0.16, 1, 0.3, 1], // быстрый старт, мягкое торможение
    })
    return () => controls.stop()
  }, [motionValue, value, reduceMotion])

  return <motion.span className={cn('tabular-nums', className)}>{text}</motion.span>
}