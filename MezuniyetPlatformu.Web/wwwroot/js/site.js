// site.js DOSYASININ SON HALİ:

// Form gönderildiğinde (Submit) Loader'ı göster
$(document).on('submit', 'form', function () {
    // Eğer form geçerliyse (validasyon hatası yoksa) loader'ı göster
    if ($(this).valid()) {
        $('#loader-wrapper').css({
            'visibility': 'visible',
            'opacity': '1'
        });
    }
});

// Sayfa kaydırıldığında Navbar gölgesini artır
window.addEventListener('scroll', function () {
    var navbar = document.getElementById('mainNavbar');
    if (navbar) {
        if (window.scrollY > 20) {
            navbar.classList.add('shadow-lg');
        } else {
            navbar.classList.remove('shadow-lg');
        }
    }
});