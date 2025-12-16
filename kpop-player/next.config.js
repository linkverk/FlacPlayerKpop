/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  swcMinify: true,
  output: 'standalone',
  
  // Отключаем автоматическую оптимизацию изображений из public
  images: {
    unoptimized: true,
  },
}

module.exports = nextConfig