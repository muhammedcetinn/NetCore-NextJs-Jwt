"use client";

import { SkeletonCard, SkeletonText } from '@/components/ui/SkeletonLoader/SkeletonLoader';
import { useAuth } from '@/context/AuthContext';
import Link from 'next/link';

/**
 * Kullanıcının temel dashboard'u.
 * Middleware tarafından korunur ve sadece kimliği doğrulanmış kullanıcılar erişebilir.
 */
export default function UserPage() {
    const { user, isLoading } = useAuth();

    // Senaryo 1: Oturum bilgileri yükleniyor.
    // Kullanıcıya sayfanın iskeletini göstererek bekleme deneyimini iyileştiriyoruz.
    if (isLoading) {
        return (
            <section>
                <SkeletonText width="300px" height="2.5rem" />
                <SkeletonText width="400px" />
                <div style={{ marginTop: '2rem' }}>
                    <SkeletonCard />
                </div>
            </section>
        );
    }

    // Senaryo 2: Middleware'den kaçmış veya oturumu aniden sonlanmış bir kullanıcı.
    // Bu durumun normalde yaşanmaması gerekir, bir güvenlik ağıdır.
    if (!user) {
        return (
            <section style={{ textAlign: 'center', marginTop: '4rem' }}>
                <h1>Erişim Reddedildi</h1>
                <p>Bu sayfayı görüntülemek için yetkiniz bulunmamaktadır.</p>
                <Link href="/" style={{ color: 'royalblue' }}>
                    Ana Sayfaya Dön
                </Link>
            </section>
        );
    }

    // Senaryo 3: Her şey yolunda, kullanıcı bilgileri yüklendi.
    return (
        <section>
            <header>
                <h1>User Dashboard</h1>
                <p>Hoş geldiniz, <strong>{user.email}</strong>.</p>
            </header>
            
            <article style={{ marginTop: '2rem' }}>
                <p>Bu sayfa, "User" veya "Admin" rolüne sahip tüm kullanıcılar tarafından görülebilir ve middleware tarafından korunmaktadır.</p>
                {/* Gelecekte buraya kullanıcıya özel component'ler (örn: <MyOrders />, <ProfileSummary />) gelebilir. */}
            </article>
        </section>
    );
}