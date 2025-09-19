import type { Metadata, Viewport } from 'next';
import { Inter } from 'next/font/google';
import { Toaster } from 'react-hot-toast';

// Component ve Provider importları
import { AuthProvider } from '@/context/AuthContext';
import Navbar from '@/components/NavBar/Navbar';
import GlobalSpinner from '@/components/GlobalSpinner/GlobalSpinner';

// Global stil dosyası
import './globals.css';

// Next.js'in tavsiye ettiği font optimizasyonu
const inter = Inter({ subsets: ['latin'], variable: '--font-inter' });

export const metadata: Metadata = {
  title: 'Next.js & .NET Auth',
  description: 'Authentication Example',
  // Yeni: Mobil cihazlar için viewport ayarı eklemek iyi bir pratiktir.
  //viewport: 'width=device-width, initial-scale=1',
};
export const viewport: Viewport = {
  width: 'device-width',
  initialScale: 1,
};
/**
 * RootLayout, tüm uygulamanın temelini oluşturur.
 * Gerekli tüm global provider'ları ve UI elemanlarını içerir.
 */
export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    // Font değişkenini <html> etiketine ekliyoruz
    <html lang="en" className={inter.variable}>
      <body>
        {/* AuthProvider, kimlik doğrulama durumunu yönetir ve en dışta olmalıdır. */}
        <AuthProvider>
          {/* Bu component'ler AuthContext'e bağlı olmadığı için sırası önemli değil */}
          <Toaster 
            position="bottom-right" 
            toastOptions={{
              // Toast'lar için varsayılan stil ayarları
              style: {
                background: '#333',
                color: '#fff',
              },
            }}
          />
          <GlobalSpinner />

          {/* Navbar, AuthContext'i kullandığı için AuthProvider içinde olmalıdır. */}
          <Navbar />
          
          {/* Sayfa içeriği 'main' etiketi içinde render edilir. */}
          <main className="main-container">
            {children}
          </main>
        </AuthProvider>
      </body>
    </html>
  );
}