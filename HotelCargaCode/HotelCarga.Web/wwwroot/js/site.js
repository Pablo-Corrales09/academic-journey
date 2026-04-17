document.addEventListener("DOMContentLoaded", () => {
	const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

	initNavbarBehavior();
	markActiveNavLink();
	initRevealAnimations(reduceMotion);

	if (!reduceMotion) {
		initPageTransitions();
	}
});

function initNavbarBehavior() {
	const header = document.querySelector("[data-navbar-header]");
	if (!header) {
		return;
	}

	const toggleScrolled = () => {
		header.classList.toggle("is-scrolled", window.scrollY > 8);
	};

	window.addEventListener("scroll", toggleScrolled, { passive: true });
	toggleScrolled();
}

function markActiveNavLink() {
	const currentPath = window.location.pathname.toLowerCase();
	const links = document.querySelectorAll(".app-nav-link");

	links.forEach((link) => {
		const href = link.getAttribute("href");
		if (!href || href.startsWith("#")) {
			return;
		}

		const targetPath = new URL(link.href, window.location.origin).pathname.toLowerCase();
		const isHome = targetPath === "/";
		const isMatch = isHome ? currentPath === "/" : currentPath.startsWith(targetPath);

		if (isMatch) {
			link.classList.add("active");
			link.setAttribute("aria-current", "page");
		}
	});
}

function initRevealAnimations(reduceMotion) {
	const targets = document.querySelectorAll("[data-animate], [data-reveal], [data-fade]");
	if (!targets.length) {
		return;
	}

	if (reduceMotion || !("IntersectionObserver" in window)) {
		targets.forEach((target) => target.classList.add("is-visible"));
		return;
	}

	const observer = new IntersectionObserver(
		(entries, obs) => {
			entries.forEach((entry) => {
				if (!entry.isIntersecting) {
					return;
				}

				entry.target.classList.add("is-visible");
				obs.unobserve(entry.target);
			});
		},
		{
			threshold: 0.16,
			rootMargin: "0px 0px -7% 0px"
		}
	);

	targets.forEach((target, index) => {
		target.style.transitionDelay = `${Math.min(index, 8) * 45}ms`;
		observer.observe(target);
	});
}

function initPageTransitions() {
	const transitionContainer = document.querySelector("[data-page-transition-container]");
	if (!transitionContainer) {
		return;
	}

	document.querySelectorAll("a[href]").forEach((link) => {
		link.addEventListener("click", (event) => {
			if (!shouldInterceptNavigation(event, link)) {
				return;
			}

			event.preventDefault();
			document.body.classList.add("is-page-leaving");

			window.setTimeout(() => {
				window.location.href = link.href;
			}, 180);
		});
	});

	window.addEventListener("pageshow", () => {
		document.body.classList.remove("is-page-leaving");
	});
}

function shouldInterceptNavigation(event, link) {
	if (event.defaultPrevented) {
		return false;
	}

	if (event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
		return false;
	}

	const href = link.getAttribute("href");
	if (!href) {
		return false;
	}

	if (link.target === "_blank" || link.hasAttribute("download")) {
		return false;
	}

	if (href.startsWith("#") || href.startsWith("javascript:") || href.startsWith("mailto:") || href.startsWith("tel:")) {
		return false;
	}

	const destination = new URL(link.href, window.location.origin);
	if (destination.origin !== window.location.origin) {
		return false;
	}

	return destination.pathname + destination.search !== window.location.pathname + window.location.search;
}