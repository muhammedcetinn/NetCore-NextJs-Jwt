"use client";

import { useAuth } from '@/context/AuthContext';
import { useRoles } from '@/hooks/useRoles'; // Rol kontrolünü merkezileştirmek için hook'umuzu kullanıyoruz
import { SkeletonCard, SkeletonText } from '@/components/ui/SkeletonLoader/SkeletonLoader';
import Link from 'next/link';

/**
 * Sadece 'Admin' rolüne sahip kullanıcıların erişebileceği yönetim paneli.
 * Middleware tarafından korunur ve ek olarak client-side rol kontrolü yapar.
 */
export default function AdminPage() {
    const { isLoading } = useAuth();
    // useAuth yerine, hem user bilgisi hem de rol kontrol mantığı içeren
    // useRoles hook'unu kullanmak daha temiz.
    const { user, isAdmin } = useRoles();

    // Senaryo 1: Oturum ve rol bilgileri yükleniyor.
    if (isLoading) {
        return (
            <section>
                <SkeletonText width="350px" height="2.5rem" />
                <SkeletonText width="500px" />
                <div style={{ marginTop: '2rem' }}>
                    <SkeletonCard />
                    <SkeletonCard />
                </div>
            </section>
        );
    }

    // Senaryo 2: Middleware'den kaçmış veya rolü client'ta doğrulanamayan bir kullanıcı.
    // Bu, hem !user durumunu hem de !isAdmin durumunu kapsar.
    if (!isAdmin) {
        return (
            <section style={{ textAlign: 'center', marginTop: '4rem' }}>
                <h1>Erişim Reddedildi</h1>
                <p>Bu sayfayı görüntülemek için Admin yetkiniz bulunmamaktadır.</p>
                <Link href="/" style={{ color: 'royalblue' }}>
                    Ana Sayfaya Dön
                </Link>
            </section>
        );
    }

    // Senaryo 3: Her şey yolunda, kullanıcı bir Admin.
    return (
        <section>
            <header>
                <h1>Admin Panel</h1>
                <p>Hoş geldiniz, Admin <strong>{user?.email}</strong>!</p> 
                {/* user? kullanarak null olma ihtimaline karşı type-safe hale getirdik */}
            </header>
            
            <article style={{ marginTop: '2rem' }}>
                <p>Bu sayfa, SADECE "Admin" rolüne sahip kullanıcılar tarafından görülebilir ve middleware tarafından korunmaktadır.</p>
                {/* 
                  Gelecekte buraya admin'e özel component'ler eklenebilir:
                  <UserManagementTable />
                  <SiteSettingsForm /> 
                */}
            </article>
        </section>
    );
}