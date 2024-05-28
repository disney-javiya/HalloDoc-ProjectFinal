
self.addEventListener('push', function (event) {
    const data = event.data.json();
    const options = {
        body: data.body,
        // icon: 'images/icon.png',
        // badge: 'images/badge.png'
    };
    event.waitUntil(
        self.registration.showNotification(data.title, options)
    );
});
