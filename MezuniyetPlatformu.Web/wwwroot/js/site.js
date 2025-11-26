// Sayfa kaydırıldığında Navbar'ın görünümünü değiştir
window.addEventListener('scroll', function () {
    var navbar = document.getElementById('mainNavbar');

    if (window.scrollY > 50) {
        // 50 pikselden fazla aşağı inildiyse 'scrolled' sınıfını ekle
        navbar.classList.add('scrolled');
    } else {
        // En tepedeyse sınıfı kaldır
        navbar.classList.remove('scrolled');
    }
});