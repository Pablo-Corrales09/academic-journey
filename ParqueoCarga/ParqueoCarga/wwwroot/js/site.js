(() => {
	const stickyHeader = document.querySelector('.js-sticky-header');
	const revealElements = document.querySelectorAll('[data-reveal]');

	const syncHeader = () => {
		if (!stickyHeader) {
			return;
		}

		stickyHeader.classList.toggle('is-compact', window.scrollY > 24);
	};

	if (stickyHeader) {
		syncHeader();
		window.addEventListener('scroll', syncHeader, { passive: true });
	}

	if (revealElements.length > 0 && 'IntersectionObserver' in window) {
		const observer = new IntersectionObserver((entries) => {
			entries.forEach((entry) => {
				if (entry.isIntersecting) {
					entry.target.classList.add('is-revealed');
					observer.unobserve(entry.target);
				}
			});
		}, {
			threshold: 0.12,
			rootMargin: '0px 0px -40px 0px'
		});

		revealElements.forEach((element) => observer.observe(element));
	} else {
		revealElements.forEach((element) => element.classList.add('is-revealed'));
	}
})();
