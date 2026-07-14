document.addEventListener('DOMContentLoaded', () => {
  // Initialize Lucide Icons
  if (typeof lucide !== 'undefined') {
    lucide.createIcons();
  }

  // ==========================================================================
  // Sticky Mobile Navigation Toggle
  // ==========================================================================
  const navToggle = document.getElementById('nav-toggle');
  const navMenu = document.getElementById('nav-menu');
  const toggleIcon = document.getElementById('toggle-icon');
  const navLinks = document.querySelectorAll('.nav-link');

  if (navToggle && navMenu) {
    navToggle.addEventListener('click', () => {
      const isOpen = navMenu.classList.toggle('open');
      
      // Toggle Menu Icon (Menu <=> Close X)
      if (toggleIcon) {
        if (isOpen) {
          toggleIcon.setAttribute('data-lucide', 'x');
        } else {
          toggleIcon.setAttribute('data-lucide', 'menu');
        }
        if (typeof lucide !== 'undefined') {
          lucide.createIcons();
        }
      }
    });

    // Close menu when clicking links on mobile
    navLinks.forEach(link => {
      link.addEventListener('click', () => {
        if (navMenu.classList.contains('open')) {
          navMenu.classList.remove('open');
          if (toggleIcon) {
            toggleIcon.setAttribute('data-lucide', 'menu');
            if (typeof lucide !== 'undefined') lucide.createIcons();
          }
        }
      });
    });
  }

  // ==========================================================================
  // Fully Playable Sudoku Engine
  // ==========================================================================
  const puzzles = [
    {
      difficulty: "Medium",
      clues: [
        5, 3, 0, 0, 7, 0, 0, 0, 0,
        6, 0, 0, 1, 9, 5, 0, 0, 0,
        0, 9, 8, 0, 0, 0, 0, 6, 0,
        8, 0, 0, 0, 6, 0, 0, 0, 3,
        4, 0, 0, 8, 0, 3, 0, 0, 1,
        7, 0, 0, 0, 2, 0, 0, 0, 6,
        0, 6, 0, 0, 0, 0, 2, 8, 0,
        0, 0, 0, 4, 1, 9, 0, 0, 5,
        0, 0, 0, 0, 8, 0, 0, 7, 9
      ],
      solution: [
        5, 3, 4, 6, 7, 8, 9, 1, 2,
        6, 7, 2, 1, 9, 5, 3, 4, 8,
        1, 9, 8, 3, 4, 2, 5, 6, 7,
        8, 5, 9, 7, 6, 1, 4, 2, 3,
        4, 2, 6, 8, 5, 3, 7, 9, 1,
        7, 1, 3, 9, 2, 4, 8, 5, 6,
        9, 6, 1, 5, 3, 7, 2, 8, 4,
        2, 8, 7, 4, 1, 9, 6, 3, 5,
        3, 4, 5, 2, 8, 6, 1, 7, 9
      ]
    },
    {
      difficulty: "Easy",
      clues: [
        0, 0, 0, 2, 6, 0, 7, 0, 1,
        6, 8, 0, 0, 7, 0, 0, 9, 0,
        1, 9, 0, 0, 0, 4, 5, 0, 0,
        8, 2, 0, 1, 0, 0, 0, 4, 0,
        0, 0, 4, 6, 0, 2, 9, 0, 0,
        0, 5, 0, 0, 0, 3, 0, 2, 8,
        0, 0, 9, 3, 0, 0, 0, 7, 4,
        0, 4, 0, 0, 5, 0, 0, 3, 6,
        7, 0, 3, 0, 1, 8, 0, 0, 0
      ],
      solution: [
        4, 3, 5, 2, 6, 9, 7, 8, 1,
        6, 8, 2, 5, 7, 1, 4, 9, 3,
        1, 9, 7, 8, 3, 4, 5, 6, 2,
        8, 2, 6, 1, 9, 5, 3, 4, 7,
        3, 7, 4, 6, 8, 2, 9, 1, 5,
        9, 5, 1, 7, 4, 3, 6, 2, 8,
        5, 1, 9, 3, 2, 6, 8, 7, 4,
        2, 4, 8, 9, 5, 7, 1, 3, 6,
        7, 6, 3, 4, 1, 8, 2, 5, 9
      ]
    }
  ];

  let currentPuzzleIdx = 0;
  let activePuzzle = null;
  let boardState = []; // Holds user inputs
  let selectedCellIdx = null;
  
  // Timer States
  let timerInterval = null;
  let elapsedSeconds = 0;

  const sudokuGrid = document.getElementById('sudoku-grid');
  const timerDisplay = document.getElementById('sudoku-time');
  const difficultyDisplay = document.querySelector('.diff-value');
  const btnReset = document.getElementById('btn-sudoku-reset');
  const btnNew = document.getElementById('btn-sudoku-new');
  const btnCheck = document.getElementById('btn-sudoku-check');
  const keyButtons = document.querySelectorAll('.key-btn');

  // Format timer values
  const formatTimer = (totalSeconds) => {
    const mins = Math.floor(totalSeconds / 60);
    const secs = totalSeconds % 60;
    return `${mins < 10 ? '0' : ''}${mins}:${secs < 10 ? '0' : ''}${secs}`;
  };

  const startTimer = () => {
    clearInterval(timerInterval);
    elapsedSeconds = 0;
    timerDisplay.textContent = formatTimer(elapsedSeconds);
    timerInterval = setInterval(() => {
      elapsedSeconds++;
      timerDisplay.textContent = formatTimer(elapsedSeconds);
    }, 1000);
  };

  const stopTimer = () => {
    clearInterval(timerInterval);
  };

  // Peer highlighting helper
  const updateHighlights = () => {
    const cells = document.querySelectorAll('.sudoku-cell');
    
    // Clear all previous highlight classes
    cells.forEach(cell => {
      cell.classList.remove('selected', 'highlighted');
    });

    if (selectedCellIdx === null) return;

    const row = Math.floor(selectedCellIdx / 9);
    const col = selectedCellIdx % 9;
    const boxRow = Math.floor(row / 3) * 3;
    const boxCol = Math.floor(col / 3) * 3;

    cells.forEach((cell, idx) => {
      const cRow = Math.floor(idx / 9);
      const cCol = idx % 9;
      const cBoxRow = Math.floor(cRow / 3) * 3;
      const cBoxCol = Math.floor(cCol / 3) * 3;

      if (idx === selectedCellIdx) {
        cell.classList.add('selected');
      } else if (cRow === row || cCol === col || (cBoxRow === boxRow && cBoxCol === boxCol)) {
        // Row, Column or Box buddy
        cell.classList.add('highlighted');
      } else if (boardState[idx] !== 0 && boardState[idx] === boardState[selectedCellIdx]) {
        // Highlight matching values
        cell.classList.add('highlighted');
      }
    });
  };

  // Render Sudoku board
  const renderBoard = () => {
    sudokuGrid.innerHTML = '';
    selectedCellIdx = null;

    boardState.forEach((val, idx) => {
      const cell = document.createElement('div');
      cell.classList.add('sudoku-cell');
      cell.setAttribute('role', 'gridcell');
      cell.setAttribute('tabindex', '0');
      
      const isClue = activePuzzle.clues[idx] !== 0;

      if (isClue) {
        cell.classList.add('clue');
        cell.textContent = val;
      } else {
        cell.textContent = val !== 0 ? val : '';
        
        // Input Selection click listener
        cell.addEventListener('click', (e) => {
          e.stopPropagation();
          selectedCellIdx = idx;
          updateHighlights();
        });

        // Keyboard interaction
        cell.addEventListener('keydown', (e) => {
          if (isClue) return;
          
          if (e.key >= '1' && e.key <= '9') {
            setCellValue(idx, parseInt(e.key));
          } else if (e.key === 'Backspace' || e.key === 'Delete' || e.key === '0') {
            setCellValue(idx, 0);
          }
        });
      }

      sudokuGrid.appendChild(cell);
    });
  };

  const setCellValue = (idx, val) => {
    if (activePuzzle.clues[idx] !== 0) return; // Cannot overwrite clues
    
    boardState[idx] = val;
    const cells = sudokuGrid.children;
    cells[idx].textContent = val !== 0 ? val : '';
    cells[idx].classList.remove('error'); // Clear errors on update
    
    updateHighlights();
  };

  // Load a new Sudoku game puzzle
  const initGame = (puzzleIndex) => {
    currentPuzzleIdx = puzzleIndex;
    activePuzzle = puzzles[currentPuzzleIdx];
    
    // Copy clues array to represent current board state
    boardState = [...activePuzzle.clues];
    
    difficultyDisplay.textContent = activePuzzle.difficulty;
    renderBoard();
    startTimer();
  };

  // Load random new game from presets
  const loadNewGame = () => {
    let nextIndex;
    do {
      nextIndex = Math.floor(Math.random() * puzzles.length);
    } while (nextIndex === currentPuzzleIdx && puzzles.length > 1);

    initGame(nextIndex);
  };

  // Reset current game
  const resetGame = () => {
    boardState = [...activePuzzle.clues];
    renderBoard();
    startTimer();
  };

  // Check board solution
  const checkSolution = () => {
    const cells = sudokuGrid.children;
    let errorsCount = 0;
    let unfilledCount = 0;

    boardState.forEach((val, idx) => {
      const correctVal = activePuzzle.solution[idx];
      cells[idx].classList.remove('error');

      if (val === 0) {
        unfilledCount++;
      } else if (val !== correctVal) {
        cells[idx].classList.add('error');
        errorsCount++;
      }
    });

    if (errorsCount === 0 && unfilledCount === 0) {
      stopTimer();
      alert(`🎉 Congratulations! You solved the Sudoku in ${formatTimer(elapsedSeconds)}!`);
    } else if (errorsCount > 0) {
      alert(`❌ Found ${errorsCount} error(s) in your grid. Keep trying!`);
    } else {
      alert(`ℹ️ No errors found so far, but the grid is incomplete. Keep filling!`);
    }
  };

  // Bind key buttons (touchpad input)
  keyButtons.forEach(btn => {
    btn.addEventListener('click', (e) => {
      e.stopPropagation();
      if (selectedCellIdx === null) return;
      
      const valAttr = btn.getAttribute('data-val');
      if (valAttr === 'clear') {
        setCellValue(selectedCellIdx, 0);
      } else {
        setCellValue(selectedCellIdx, parseInt(valAttr));
      }
    });
  });

  // Clear selections when clicking outside the grid
  document.addEventListener('click', () => {
    selectedCellIdx = null;
    updateHighlights();
  });

  // Action listeners
  btnReset.addEventListener('click', resetGame);
  btnNew.addEventListener('click', loadNewGame);
  btnCheck.addEventListener('click', checkSolution);

  // Initialize first game on load
  initGame(0);

  // ==========================================================================
  // Dynamic Motivational Quotes Widget
  // ==========================================================================
  const quotesPool = [
    { text: "The best way to predict the future is to create it.", author: "Abraham Lincoln" },
    { text: "The only way to do great work is to love what you do.", author: "Steve Jobs" },
    { text: "Opportunities don't happen, you create them.", author: "Chris Grosser" },
    { text: "Believe you can and you're halfway there.", author: "Theodore Roosevelt" },
    { text: "Success is not final, failure is not fatal: it is the courage to continue that counts.", author: "Winston Churchill" },
    { text: "Everything you've ever wanted is on the other side of fear.", author: "George Addair" },
    { text: "The expert in anything was once a beginner.", author: "Helen Hayes" },
    { text: "Do not wait for opportunities, write your own story.", author: "Unknown" }
  ];

  const quoteContent = document.getElementById('quote-content');
  const quoteAuthor = document.getElementById('quote-author');
  const btnRefreshQuote = document.getElementById('btn-refresh-quote');
  
  let currentQuoteIdx = -1;

  const setRandomQuote = () => {
    let nextIdx;
    do {
      nextIdx = Math.floor(Math.random() * quotesPool.length);
    } while (nextIdx === currentQuoteIdx && quotesPool.length > 1);

    currentQuoteIdx = nextIdx;
    const quote = quotesPool[currentQuoteIdx];

    if (quoteContent && quoteAuthor) {
      // Fade out
      quoteContent.classList.add('quote-fade-out');
      quoteAuthor.classList.add('quote-fade-out');

      setTimeout(() => {
        quoteContent.textContent = `"${quote.text}"`;
        quoteAuthor.textContent = `— ${quote.author}`;
        
        // Fade back in
        quoteContent.classList.remove('quote-fade-out');
        quoteAuthor.classList.remove('quote-fade-out');
      }, 250);
    }
  };

  if (btnRefreshQuote) {
    btnRefreshQuote.addEventListener('click', (e) => {
      e.stopPropagation();
      setRandomQuote();
      
      // Spin icon feedback
      const icon = document.getElementById('refresh-icon');
      if (icon) {
        icon.style.transform = 'rotate(360deg)';
        icon.style.transition = 'transform 0.5s ease';
        setTimeout(() => {
          icon.style.transform = '';
          icon.style.transition = '';
        }, 500);
      }
    });
  }

  // Set initial quote
  setRandomQuote();

  // ==========================================================================
  // Background Music Controller
  // ==========================================================================
  // PLAYLIST CONFIGURATION:
  // To replace these tracks with your own licensed music in the future:
  // 1. Store your custom MP3 files inside the "/music" folder in this project directory.
  // 2. Update the array below with the relative file paths of your music files 
  //    (e.g., "music/your-custom-song.mp3").
  // 3. Always verify that your custom files are royalty-free, public domain, or 
  //    properly licensed for commercial distribution.
  const bgMusicPlaylist = [
    "music/track1.mp3",
    "music/track2.mp3"
  ];
  
  let currentBgMusicIdx = 0;
  const bgAudio = new Audio(bgMusicPlaylist[currentBgMusicIdx]);
  bgAudio.volume = 0.3; // Default volume set to 30%
  bgAudio.loop = false; // We sequence it, so we loop the playlist manually

  const btnMusicToggle = document.getElementById('btn-music-toggle');
  const btnMusicMute = document.getElementById('btn-music-mute');
  const musicBtnIcon = document.getElementById('music-btn-icon');
  const musicMuteIcon = document.getElementById('music-mute-icon');
  const volumeSlider = document.getElementById('music-volume-slider');
  const autoplayOverlay = document.getElementById('autoplay-overlay');
  
  let isMusicPlaying = false;
  let preMuteVolume = 0.3;

  const updateMusicIcons = () => {
    if (musicBtnIcon) {
      musicBtnIcon.setAttribute('data-lucide', isMusicPlaying ? 'pause' : 'play');
    }
    if (musicMuteIcon) {
      const isMuted = bgAudio.volume === 0;
      musicMuteIcon.setAttribute('data-lucide', isMuted ? 'volume-x' : bgAudio.volume < 0.4 ? 'volume-1' : 'volume-2');
    }
    if (btnMusicToggle) {
      if (isMusicPlaying) {
        btnMusicToggle.classList.add('playing');
      } else {
        btnMusicToggle.classList.remove('playing');
      }
    }
    if (typeof lucide !== 'undefined') {
      lucide.createIcons();
    }
  };

  const playBgMusic = () => {
    bgAudio.play().then(() => {
      isMusicPlaying = true;
      updateMusicIcons();
      if (autoplayOverlay) autoplayOverlay.classList.remove('show');
    }).catch(error => {
      console.log("Autoplay blocked:", error);
      if (autoplayOverlay) autoplayOverlay.classList.add('show');
      
      // Trigger play on first interaction
      const startOnInteraction = () => {
        bgAudio.play().then(() => {
          isMusicPlaying = true;
          updateMusicIcons();
          if (autoplayOverlay) autoplayOverlay.classList.remove('show');
          
          // Clear interaction listeners
          document.removeEventListener('click', startOnInteraction);
          document.removeEventListener('keydown', startOnInteraction);
        }).catch(err => console.log("Play on interaction failed:", err));
      };
      
      document.addEventListener('click', startOnInteraction);
      document.addEventListener('keydown', startOnInteraction);
    });
  };

  const pauseBgMusic = () => {
    bgAudio.pause();
    isMusicPlaying = false;
    updateMusicIcons();
  };

  // Toggle Music play state on main button click
  if (btnMusicToggle) {
    btnMusicToggle.addEventListener('click', (e) => {
      e.stopPropagation();
      if (isMusicPlaying) {
        pauseBgMusic();
      } else {
        playBgMusic();
      }
    });
  }

  // Sequence playlist on song end
  bgAudio.addEventListener('ended', () => {
    currentBgMusicIdx = (currentBgMusicIdx + 1) % bgMusicPlaylist.length;
    bgAudio.src = bgMusicPlaylist[currentBgMusicIdx];
    // Keep volume at current setting
    playBgMusic();
  });

  // Mute / Unmute
  const toggleMute = () => {
    const isMuted = bgAudio.volume === 0;
    if (isMuted) {
      // Unmute: restore previous volume
      bgAudio.volume = preMuteVolume > 0 ? preMuteVolume : 0.3;
      if (volumeSlider) volumeSlider.value = Math.round(bgAudio.volume * 100);
    } else {
      // Mute: save current volume and set to 0
      preMuteVolume = bgAudio.volume;
      bgAudio.volume = 0;
      if (volumeSlider) volumeSlider.value = 0;
    }
    updateMusicIcons();
  };

  if (btnMusicMute) {
    btnMusicMute.addEventListener('click', (e) => {
      e.stopPropagation();
      toggleMute();
    });
  }

  // Volume slider control
  if (volumeSlider) {
    volumeSlider.addEventListener('input', (e) => {
      e.stopPropagation();
      const val = parseInt(e.target.value);
      bgAudio.volume = val / 100;
      if (val > 0) {
        preMuteVolume = bgAudio.volume;
      }
      updateMusicIcons();
    });
  }

  // Start background music automatically on load
  playBgMusic();

  // ==========================================================================
  // API Gateway Fetching and Rendering
  // ==========================================================================
  const getCompanyLogoSvg = (logoType) => {
    switch (logoType) {
      case 'microsoft':
        return `<div class="company-logo microsoft-bg">
                  <svg viewBox="0 0 23 23" width="22" height="22">
                    <rect x="0" y="0" width="10.5" height="10.5" fill="#f25022"/>
                    <rect x="12.5" y="0" width="10.5" height="10.5" fill="#7fba00"/>
                    <rect x="0" y="12.5" width="10.5" height="10.5" fill="#00a4ef"/>
                    <rect x="12.5" y="12.5" width="10.5" height="10.5" fill="#ffb900"/>
                  </svg>
                </div>`;
      case 'tcs':
        return `<div class="company-logo tcs-bg">
                  <svg viewBox="0 0 100 100" width="22" height="22">
                    <circle cx="50" cy="50" r="45" fill="none" stroke="currentColor" stroke-width="8"/>
                    <path d="M35,38 L65,38 M50,38 L50,70" stroke="currentColor" stroke-width="10" stroke-linecap="round" fill="none"/>
                  </svg>
                </div>`;
      case 'amazon':
        return `<div class="company-logo amazon-bg">
                  <svg viewBox="0 0 100 100" width="22" height="22">
                    <path d="M20,60 C35,78 65,78 80,60" fill="none" stroke="#ff9900" stroke-width="10" stroke-linecap="round"/>
                    <path d="M72,55 L83,58 L80,70" fill="#ff9900" stroke="#ff9900" stroke-width="2" stroke-linejoin="round"/>
                  </svg>
                </div>`;
      case 'deloitte':
        return `<div class="company-logo deloitte-bg">
                  <svg viewBox="0 0 100 100" width="20" height="20">
                    <circle cx="50" cy="50" r="45" fill="none" stroke="currentColor" stroke-width="8"/>
                    <circle cx="70" cy="70" r="12" fill="#86bc25"/>
                  </svg>
                </div>`;
      case 'nova':
        return `<div class="company-logo nova-bg">
                  <svg viewBox="0 0 100 100" width="22" height="22">
                    <polygon points="50,15 90,85 10,85" fill="none" stroke="currentColor" stroke-width="8"/>
                    <circle cx="50" cy="55" r="15" fill="currentColor"/>
                  </svg>
                </div>`;
      case 'cloudzapier':
        return `<div class="company-logo cloudzapier-bg">
                  <svg viewBox="0 0 100 100" width="22" height="22">
                    <path d="M50,15 L80,30 L80,60 C80,75 50,85 50,85 C50,85 20,75 20,60 L20,30 Z" fill="none" stroke="currentColor" stroke-width="8" stroke-linejoin="round"/>
                    <path d="M40,50 L48,58 L62,44" fill="none" stroke="currentColor" stroke-width="8" stroke-linecap="round" stroke-linejoin="round"/>
                  </svg>
                </div>`;
      default:
        return `<div class="company-logo" style="background-color: rgba(255, 255, 255, 0.04); color: var(--accent-color); border: 1px solid rgba(255, 255, 255, 0.08);">
                  <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <rect width="20" height="14" x="2" y="7" rx="2" ry="2"/>
                    <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/>
                  </svg>
                </div>`;
    }
  };

  const renderOpportunities = (opportunities, append = false) => {
    const container = document.getElementById('opportunities-container');
    if (!container) return;
    if (!append) {
      container.innerHTML = '';
    }

    opportunities.forEach(opt => {
      const tagClass = opt.tag === 'FREE' ? 'badge-success-tag' : 'badge-tag';
      const lastDateHtml = opt.lastDate ? `
              <div class="meta-item">
                <i data-lucide="hourglass" class="meta-icon"></i>
                <div class="meta-text">
                  <span class="meta-label">Last Date</span>
                  <span class="meta-value">${opt.lastDate}</span>
                </div>
              </div>` : '';
              
      const card = document.createElement('article');
      card.className = 'job-card';
      card.innerHTML = `
        <div class="card-header">
          <div class="company-brand">
            ${getCompanyLogoSvg(opt.logoType)}
            <div class="company-info">
              <span class="company-name">${opt.company}</span>
              <h3 class="role-title">${opt.role}</h3>
            </div>
          </div>
          <span class="badge ${tagClass}">${opt.tag}</span>
        </div>

        <div class="card-body">
          <div class="meta-grid">
            <div class="meta-item">
              <i data-lucide="graduation-cap" class="meta-icon"></i>
              <div class="meta-text">
                <span class="meta-label">Eligibility</span>
                <span class="meta-value">${opt.eligibility}</span>
              </div>
            </div>
            <div class="meta-item">
              <i data-lucide="circle-dollar-sign" class="meta-icon"></i>
              <div class="meta-text">
                <span class="meta-label">${opt.tag === 'FREE' ? 'Cost' : 'Stipend'}</span>
                <span class="meta-value">${opt.stipend}</span>
              </div>
            </div>
            <div class="meta-item">
              <i data-lucide="calendar" class="meta-icon"></i>
              <div class="meta-text">
                <span class="meta-label">Duration</span>
                <span class="meta-value">${opt.duration}</span>
              </div>
            </div>
            <div class="meta-item">
              <i data-lucide="map-pin" class="meta-icon"></i>
              <div class="meta-text">
                <span class="meta-label">Mode & Location</span>
                <span class="meta-value">${opt.mode}</span>
              </div>
            </div>
            ${lastDateHtml}
          </div>
        </div>

        <div class="card-footer">
          <a href="${opt.applyUrl}" target="_blank" rel="noopener noreferrer" class="btn-apply">
            <span>${opt.tag === 'FREE' ? 'Start Simulation' : 'Apply Now'}</span>
            <i data-lucide="arrow-up-right"></i>
          </a>
        </div>
      `;
      container.appendChild(card);
    });

    if (typeof lucide !== 'undefined') {
      lucide.createIcons();
    }
  };

  const renderJobs = (jobs, append = false) => {
    const container = document.getElementById('jobs-container');
    if (!container) return;
    if (!append) {
      container.innerHTML = '';
    } else {
      container.querySelectorAll('.job-row-coming').forEach(row => row.remove());
    }

    jobs.forEach(job => {
      const row = document.createElement('tr');
      row.innerHTML = `
        <td data-label="Company" style="font-weight: 700; color: var(--text-main);">${job.company}</td>
        <td data-label="Role" style="font-weight: 600; color: var(--text-main);">${job.role}</td>
        <td data-label="Location" style="color: var(--text-light);">${job.location}</td>
        <td data-label="Action">
          <a href="${job.applyUrl}" target="_blank" rel="noopener noreferrer" class="btn-table-apply">
            <span>Apply Now</span>
            <i data-lucide="arrow-up-right"></i>
          </a>
        </td>
      `;
      container.appendChild(row);
    });

    // Re-append placeholders to the bottom
    container.querySelectorAll('.job-row-coming').forEach(row => row.remove());
    const placeholders = [
      { company: "Google", role: "Associate Product Manager", location: "Bangalore, IN" },
      { company: "Meta", role: "Data Engineer (University Grad)", location: "London, UK" },
      { company: "Stripe", role: "Software Engineer (L3)", location: "Remote (US/Canada)" }
    ];

    placeholders.forEach(ph => {
      const row = document.createElement('tr');
      row.className = 'job-row-coming';
      row.innerHTML = `
        <td data-label="Company"><span class="blur-text">${ph.company}</span></td>
        <td data-label="Role"><span class="blur-text">${ph.role}</span></td>
        <td data-label="Location"><span class="blur-text">${ph.location}</span></td>
        <td data-label="Action">
          <button class="btn-table-coming" disabled>Coming Soon</button>
        </td>
      `;
      container.appendChild(row);
    });

    if (typeof lucide !== 'undefined') {
      lucide.createIcons();
    }
  };

  // Local database fallbacks for high availability (offline or direct-load failures)
  const localFallbackOpportunities = [
    {
      id: "microsoft-swe",
      company: "Microsoft",
      role: "Software Engineering Intern",
      eligibility: "Bachelor's/Master's, CS/Engineering/IT",
      stipend: "Industry Standard",
      duration: "8–12 weeks",
      mode: "Full-time, On-site",
      applyUrl: "https://careers.microsoft.com/v2/global/en/students",
      tag: "SWE",
      logoType: "microsoft",
      lastDate: null
    },
    {
      id: "tcs-research",
      company: "TCS",
      role: "Research / On-Site Internship",
      eligibility: "Final-year B.Tech, strong academic record",
      stipend: "₹20,000–₹40,000/month",
      duration: "12 weeks",
      mode: "On-site / Hybrid",
      applyUrl: "https://www.tcs.com/careers/india/internship",
      tag: "Research",
      logoType: "tcs",
      lastDate: null
    },
    {
      id: "amazon-sde",
      company: "Amazon",
      role: "Software Development Engineer Intern",
      eligibility: "Bachelor's+ in CS/STEM, coding fundamentals",
      stipend: "₹80,000–₹1.2L/month equivalent",
      duration: "12 weeks (Fall 2026)",
      mode: "On-site (US)",
      applyUrl: "https://www.amazon.jobs/en/jobs/3116030/software-development-engineer-internship-fall-2026-us",
      tag: "SWE",
      logoType: "amazon",
      lastDate: null
    },
    {
      id: "deloitte-data",
      company: "Deloitte",
      role: "Data Analytics Job Simulation",
      eligibility: "Open to all students, no experience required",
      stipend: "Fully Free · Virtual Experience",
      duration: "Self-paced (Forage, AU)",
      mode: "Fully Virtual",
      applyUrl: "https://www.theforage.com/simulations/deloitte-au/data-analytics-s5zy",
      tag: "FREE",
      logoType: "deloitte",
      lastDate: null
    },
    {
      id: "nova-robotics",
      company: "Nova Robotics",
      role: "Aerospace / Mechanical R&D Intern",
      eligibility: "Aerospace / Mechanical candidates",
      stipend: "₹10,000–₹50,000/month",
      duration: "3–6 months",
      mode: "In-Office",
      applyUrl: "https://unstop.com/internships/aerospace-mechanical-rd-internship-nova-robotics-private-limited-1717688?lb=krHMyqHr&utm_medium=Share&utm_source=internships&utm_campaign=Ysatyam610",
      tag: "R&D",
      logoType: "nova",
      lastDate: "20 Jul 2026"
    },
    {
      id: "cloudzapier-security",
      company: "Cloudzapier",
      role: "Cyber Security Intern",
      eligibility: "Cyber Security / CS / IT candidates",
      stipend: "₹10,000–₹20,000/month",
      duration: "3 months",
      mode: "Work From Home",
      applyUrl: "https://internshala.com/internship/detail/work-from-home-cyber-security-internship-at-cloudzapier1783831728",
      tag: "Security",
      logoType: "cloudzapier",
      lastDate: "11 Aug 2026"
    }
  ];

  const localFallbackJobs = [
    {
      company: "Accenture",
      role: "I&F Decision Sci Practitioner Associate",
      location: "Navi Mumbai, IN",
      applyUrl: "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01653459_en&title=I%26F+Decision+Sci+Practitioner+Associate"
    },
    {
      company: "Accenture",
      role: "Analytics and Modeling Associate",
      location: "Gurugram, IN",
      applyUrl: "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01652892_en&title=Analytics+and+Modeling+Associate"
    },
    {
      company: "Accenture",
      role: "Analytics and Modeling Associate",
      location: "Gurugram, IN",
      applyUrl: "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01652283_en&title=Analytics+and+Modeling+Associate"
    },
    {
      company: "Accenture",
      role: "Analytics and Modeling Associate",
      location: "Gurugram, IN",
      applyUrl: "https://www.accenture.com/in-en/careers/jobdetails?id=AIOC-S01651763_en&title=Analytics+and+Modeling+Associate"
    },
    {
      company: "Accenture",
      role: "Infra Tech Support Practitioner",
      location: "Indore, IN",
      applyUrl: "https://www.accenture.com/in-en/careers/jobdetails?id=ATCI-5288362-S1948498_en&title=Infra+Tech+Support+Practitioner"
    },
    {
      company: "Accenture",
      role: "Technology Support Engineer",
      location: "Indore, IN",
      applyUrl: "https://www.accenture.com/in-en/careers/jobdetails?id=ATCI-5303823-S1970443_en&title=Technology+Support+Engineer"
    },
    {
      company: "Accenture",
      role: "Application Support Engineer",
      location: "Hyderabad, IN",
      applyUrl: "https://www.accenture.com/in-en/careers/jobdetails?id=14422158_en&title=Application+Support+Engineer"
    }
  ];

  const loadOpportunities = async (searchQuery = "internship", countryCode = "in", page = 1, append = false) => {
    // Helper: combine featured + API results (deduplicate by company name)
    const combineWithFeatured = (apiResults) => {
      if (page > 1) return apiResults;
      const featuredCompanies = new Set(localFallbackOpportunities.map(o => o.company.toLowerCase()));
      const dedupedApi = apiResults.filter(o => !featuredCompanies.has((o.company || '').toLowerCase()));
      return [...localFallbackOpportunities, ...dedupedApi];
    };

    // Stage 1: Try C# Backend (relative path — same server serves frontend + API)
    try {
      const res = await fetch(`/api/opportunities?q=${encodeURIComponent(searchQuery)}&country=${encodeURIComponent(countryCode)}&page=${page}`);
      if (res.ok) {
        const data = await res.json();
        // Always show featured internships first, then live API results
        renderOpportunities(combineWithFeatured(data), append);
        console.log("[Knowverse] Loaded opportunities from C# ASP.NET Core backend.");
        updateLoadMoreButton("load-more-opportunities-btn", data.length);
        return;
      }
    } catch (e) {
      console.warn("[Knowverse] C# backend unavailable. Falling back to direct Adzuna API fetch...", e);
    }

    // Stage 2: Try Direct Adzuna API
    try {
      const appId = "ad843124";
      const appKey = "84173c51c3d2cc118bca43dc9df0ad1c";
      const res = await fetch(`https://api.adzuna.com/v1/api/jobs/${countryCode}/search/${page}?app_id=${appId}&app_key=${appKey}&what=${encodeURIComponent(searchQuery)}&content-type=application/json`);
      if (res.ok) {
        const payload = await res.json();
        if (payload && payload.results) {
          const mapped = payload.results.slice(0, 6).map(item => {
            const id = (item.id || Math.random()).toString();
            const company = item.company?.display_name || "Unknown Company";
            const role = item.title || "Internship Role";
            const location = item.location?.display_name || "Remote / India";
            
            let tag = "Intern";
            const roleLower = role.toLowerCase();
            if (roleLower.includes("software") || roleLower.includes("engineering") || roleLower.includes("developer")) tag = "SWE";
            else if (roleLower.includes("data") || roleLower.includes("analytics")) tag = "Data";
            else if (roleLower.includes("design") || roleLower.includes("creative")) tag = "Design";
            else if (roleLower.includes("security") || roleLower.includes("cyber")) tag = "Security";

            let logoType = "generic";
            const compLower = company.toLowerCase();
            if (compLower.includes("microsoft")) logoType = "microsoft";
            else if (compLower.includes("amazon")) logoType = "amazon";
            else if (compLower.includes("deloitte")) logoType = "deloitte";
            else if (compLower.includes("tcs") || compLower.includes("tata consultancy")) logoType = "tcs";
            else if (compLower.includes("nova")) logoType = "nova";
            else if (compLower.includes("cloudzapier")) logoType = "cloudzapier";

            return {
              id,
              company,
              role,
              eligibility: "Tech / STEM candidates",
              stipend: "Competitive Stipend",
              duration: "3–6 months",
              mode: location,
              applyUrl: item.redirect_url || "https://www.adzuna.in",
              tag,
              logoType,
              lastDate: null
            };
          });
          renderOpportunities(combineWithFeatured(mapped), append);
          console.log("[Knowverse Gateway] Loaded opportunities directly from Adzuna API.");
          updateLoadMoreButton("load-more-opportunities-btn", mapped.length);
          return;
        }
      }
    } catch (e) {
      console.warn("[Knowverse Gateway] Direct Adzuna API fetch failed. Trying JSearch API...", e);
    }

    // Stage 3: Try Direct JSearch API (RapidAPI)
    try {
      const apiHost = "jsearch.p.rapidapi.com";
      const apiKey = "7c9340a0b7mshd4f471766795179p161947jsnda2a0b78f8c1";
      const res = await fetch(`https://${apiHost}/search?query=${encodeURIComponent(searchQuery)}&location=${countryCode}&page=${page}&num_pages=1`, {
        method: 'GET',
        headers: {
          'x-rapidapi-key': apiKey,
          'x-rapidapi-host': apiHost
        }
      });
      if (res.ok) {
        const payload = await res.json();
        if (payload && payload.data) {
          const mapped = payload.data.slice(0, 6).map(item => {
            const id = item.job_id || Math.random().toString();
            const company = item.employer_name || "Unknown Company";
            const role = item.job_title || "Internship Role";
            const city = item.job_city;
            const country = item.job_country;
            const location = (city && country) ? `${city}, ${country}` : "Remote";
            
            let tag = "Intern";
            const roleLower = role.toLowerCase();
            if (roleLower.includes("software") || roleLower.includes("engineering") || roleLower.includes("developer")) tag = "SWE";
            else if (roleLower.includes("data") || roleLower.includes("analytics")) tag = "Data";
            else if (roleLower.includes("design") || roleLower.includes("creative")) tag = "Design";
            else if (roleLower.includes("security") || roleLower.includes("cyber")) tag = "Security";

            let logoType = "generic";
            const compLower = company.toLowerCase();
            if (compLower.includes("microsoft")) logoType = "microsoft";
            else if (compLower.includes("amazon")) logoType = "amazon";
            else if (compLower.includes("deloitte")) logoType = "deloitte";
            else if (compLower.includes("tcs") || compLower.includes("tata consultancy")) logoType = "tcs";
            else if (compLower.includes("nova")) logoType = "nova";
            else if (compLower.includes("cloudzapier")) logoType = "cloudzapier";

            return {
              id,
              company,
              role,
              eligibility: "Tech / STEM candidates",
              stipend: "Competitive Stipend",
              duration: "3–6 months",
              mode: location,
              applyUrl: item.job_apply_link || "https://jsearch.p.rapidapi.com",
              tag,
              logoType,
              lastDate: null
            };
          });
          renderOpportunities(combineWithFeatured(mapped), append);
          console.log("[Knowverse Gateway] Loaded opportunities directly from JSearch API.");
          updateLoadMoreButton("load-more-opportunities-btn", mapped.length);
          return;
        }
      }
    } catch (e) {
      console.warn("[Knowverse Gateway] Direct JSearch API fetch failed. Trying Remotive API...", e);
    }

    // Stage 4: Try Direct Remotive API
    try {
      const res = await fetch("https://remotive.com/api/remote-jobs?category=software-development");
      if (res.ok) {
        const payload = await res.json();
        if (payload && payload.jobs) {
          const filteredJobs = payload.jobs.filter(item => {
            const roleLower = (item.title || '').toLowerCase();
            if (searchQuery === "internship") {
              return roleLower.includes("intern");
            }
            return roleLower.includes(searchQuery.toLowerCase());
          });
          const mapped = filteredJobs.slice((page - 1) * 6, page * 6).map(item => {
            const id = (item.id || Math.random()).toString();
            const company = item.company_name || "Unknown Company";
            const role = item.title || "Internship Role";
            const location = item.candidate_required_location || "Remote";
            
            let tag = "Intern";
            const roleLower = role.toLowerCase();
            if (roleLower.includes("software") || roleLower.includes("engineering") || roleLower.includes("developer")) tag = "SWE";
            else if (roleLower.includes("data") || roleLower.includes("analytics")) tag = "Data";
            else if (roleLower.includes("design") || roleLower.includes("creative")) tag = "Design";
            else if (roleLower.includes("security") || roleLower.includes("cyber")) tag = "Security";

            let logoType = "generic";
            const compLower = company.toLowerCase();
            if (compLower.includes("microsoft")) logoType = "microsoft";
            else if (compLower.includes("amazon")) logoType = "amazon";
            else if (compLower.includes("deloitte")) logoType = "deloitte";
            else if (compLower.includes("tcs") || compLower.includes("tata consultancy")) logoType = "tcs";
            else if (compLower.includes("nova")) logoType = "nova";
            else if (compLower.includes("cloudzapier")) logoType = "cloudzapier";

            return {
              id,
              company,
              role,
              eligibility: "Tech / STEM candidates",
              stipend: "Competitive Stipend",
              duration: "3–6 months",
              mode: location,
              applyUrl: item.url || "https://remotive.com",
              tag,
              logoType,
              lastDate: null
            };
          });
          renderOpportunities(combineWithFeatured(mapped), append);
          console.log("[Knowverse Gateway] Loaded opportunities directly from Remotive API.");
          updateLoadMoreButton("load-more-opportunities-btn", mapped.length);
          return;
        }
      }
    } catch (e) {
      console.warn("[Knowverse Gateway] Direct Remotive API fetch failed. Trying Muse API...", e);
    }

    // Stage 5: Try Direct Muse API
    try {
      const apiKey = "e267525eb7f40f0bdee9a81184697d495998da702a5742cc233667e688e74212";
      let url = `https://www.themuse.com/api/public/jobs?level=Internship&page=${page - 1}&api_key=${apiKey}`;
      if (searchQuery !== "internship") {
        url += `&desc=${encodeURIComponent(searchQuery)}`;
      }
      const res = await fetch(url);
      if (res.ok) {
        const payload = await res.json();
        if (payload && payload.results) {
          const mapped = payload.results.slice(0, 6).map(item => {
            const id = item.id.toString();
            const company = item.company?.name || "Unknown Company";
            const role = item.name || "Internship Role";
            const location = item.locations?.[0]?.name || "Remote";
            const category = item.categories?.[0]?.name || "Tech";
            
            let tag = "Intern";
            if (category.includes("Software") || category.includes("Engineering") || category.includes("Developer")) tag = "SWE";
            else if (category.includes("Data") || category.includes("Analytics")) tag = "Data";
            else if (category.includes("Design") || category.includes("Creative")) tag = "Design";
            else if (category.includes("Security") || category.includes("Cyber")) tag = "Security";

            let logoType = "generic";
            const compLower = company.toLowerCase();
            if (compLower.includes("microsoft")) logoType = "microsoft";
            else if (compLower.includes("amazon")) logoType = "amazon";
            else if (compLower.includes("deloitte")) logoType = "deloitte";
            else if (compLower.includes("tcs") || compLower.includes("tata consultancy")) logoType = "tcs";
            else if (compLower.includes("nova")) logoType = "nova";
            else if (compLower.includes("cloudzapier")) logoType = "cloudzapier";

            return {
              id,
              company,
              role,
              eligibility: "Tech / STEM candidates",
              stipend: "Competitive Stipend",
              duration: "3–6 months",
              mode: location,
              applyUrl: item.refs?.landing_page || "https://www.themuse.com",
              tag,
              logoType,
              lastDate: null
            };
          });
          // Always show featured internships first, then live Muse API results
          renderOpportunities(combineWithFeatured(mapped), append);
          console.log("[Knowverse Gateway] Loaded opportunities directly from The Muse API.");
          updateLoadMoreButton("load-more-opportunities-btn", mapped.length);
          return;
        }
      }
    } catch (e) {
      console.warn("[Knowverse Gateway] Direct Muse API fetch failed. Using local client fallbacks...", e);
    }

    // Stage 6: Client Fallbacks (Static database — always our curated internships)
    if (!append) {
      renderOpportunities(localFallbackOpportunities, false);
      updateLoadMoreButton("load-more-opportunities-btn", 0);
    }
  };

  const loadJobs = async (searchQuery = "software developer entry level", countryCode = "in", page = 1, append = false) => {
    // Helper: combine featured jobs + API results (deduplicate by company+role)
    const combineJobsWithFeatured = (apiResults) => {
      if (page > 1) return apiResults;
      const featuredKeys = new Set(
        localFallbackJobs.map(j => `${j.company.toLowerCase()}|${j.role.toLowerCase()}`)
      );
      const dedupedApi = apiResults.filter(j =>
        !featuredKeys.has(`${(j.company || '').toLowerCase()}|${(j.role || '').toLowerCase()}`)
      );
      return [...localFallbackJobs, ...dedupedApi];
    };

    // Stage 1: Try C# Backend (relative path — same server serves frontend + API)
    try {
      const res = await fetch(`/api/jobs?q=${encodeURIComponent(searchQuery)}&country=${encodeURIComponent(countryCode)}&page=${page}`);
      if (res.ok) {
        const data = await res.json();
        // Always show featured jobs first, then live API results
        renderJobs(combineJobsWithFeatured(data), append);
        console.log("[Knowverse] Loaded jobs from C# ASP.NET Core backend.");
        updateLoadMoreButton("load-more-jobs-btn", data.length);
        return;
      }
    } catch (e) {
      console.warn("[Knowverse] C# backend unavailable. Falling back to direct Adzuna API fetch...", e);
    }

    // Stage 2: Try Direct Adzuna API
    try {
      const appId = "ad843124";
      const appKey = "84173c51c3d2cc118bca43dc9df0ad1c";
      const res = await fetch(`https://api.adzuna.com/v1/api/jobs/${countryCode}/search/${page}?app_id=${appId}&app_key=${appKey}&what=${encodeURIComponent(searchQuery)}&content-type=application/json`);
      if (res.ok) {
        const payload = await res.json();
        if (payload && payload.results) {
          const mapped = payload.results.slice(0, 7).map(item => {
            return {
              company: item.company?.display_name || "Unknown Company",
              role: item.title || "Job Vacancy",
              location: item.location?.display_name || "Various Locations",
              applyUrl: item.redirect_url || "https://www.adzuna.in"
            };
          });
          renderJobs(combineJobsWithFeatured(mapped), append);
          console.log("[Knowverse Gateway] Loaded jobs directly from Adzuna API.");
          updateLoadMoreButton("load-more-jobs-btn", mapped.length);
          return;
        }
      }
    } catch (e) {
      console.warn("[Knowverse Gateway] Direct Adzuna API fetch failed. Trying JSearch API...", e);
    }

    // Stage 3: Try Direct JSearch API (RapidAPI)
    try {
      const apiHost = "jsearch.p.rapidapi.com";
      const apiKey = "7c9340a0b7mshd4f471766795179p161947jsnda2a0b78f8c1";
      const res = await fetch(`https://${apiHost}/search?query=${encodeURIComponent(searchQuery)}&location=${countryCode}&page=${page}&num_pages=1`, {
        method: 'GET',
        headers: {
          'x-rapidapi-key': apiKey,
          'x-rapidapi-host': apiHost
        }
      });
      if (res.ok) {
        const payload = await res.json();
        if (payload && payload.data) {
          const mapped = payload.data.slice(0, 7).map(item => {
            const city = item.job_city;
            const country = item.job_country;
            return {
              company: item.employer_name || "Unknown Company",
              role: item.job_title || "Job Vacancy",
              location: (city && country) ? `${city}, ${country}` : "Various Locations",
              applyUrl: item.job_apply_link || "https://jsearch.p.rapidapi.com"
            };
          });
          renderJobs(combineJobsWithFeatured(mapped), append);
          console.log("[Knowverse Gateway] Loaded jobs directly from JSearch API.");
          updateLoadMoreButton("load-more-jobs-btn", mapped.length);
          return;
        }
      }
    } catch (e) {
      console.warn("[Knowverse Gateway] Direct JSearch API fetch failed. Trying Remotive API...", e);
    }

    // Stage 4: Try Direct Remotive API
    try {
      const res = await fetch("https://remotive.com/api/remote-jobs?category=software-development");
      if (res.ok) {
        const payload = await res.json();
        if (payload && payload.jobs) {
          const filteredJobs = payload.jobs.filter(item => {
            const roleLower = (item.title || '').toLowerCase();
            if (searchQuery === "software developer entry level") return true;
            return roleLower.includes(searchQuery.toLowerCase());
          });
          const mapped = filteredJobs.slice((page - 1) * 7, page * 7).map(item => {
            return {
              company: item.company_name || "Unknown Company",
              role: item.title || "Job Vacancy",
              location: item.candidate_required_location || "Various Locations",
              applyUrl: item.url || "https://remotive.com"
            };
          });
          renderJobs(combineJobsWithFeatured(mapped), append);
          console.log("[Knowverse Gateway] Loaded jobs directly from Remotive API.");
          updateLoadMoreButton("load-more-jobs-btn", mapped.length);
          return;
        }
      }
    } catch (e) {
      console.warn("[Knowverse Gateway] Direct Remotive API fetch failed. Trying Muse API...", e);
    }

    // Stage 5: Try Direct Muse API
    try {
      const apiKey = "e267525eb7f40f0bdee9a81184697d495998da702a5742cc233667e688e74212";
      let url = `https://www.themuse.com/api/public/jobs?level=Entry%20Level&page=${page - 1}&api_key=${apiKey}`;
      if (searchQuery !== "software developer entry level") {
        url += `&desc=${encodeURIComponent(searchQuery)}`;
      }
      const res = await fetch(url);
      if (res.ok) {
        const payload = await res.json();
        if (payload && payload.results) {
          const mapped = payload.results.slice(0, 7).map(item => {
            return {
              company: item.company?.name || "Unknown Company",
              role: item.name || "Job Vacancy",
              location: item.locations?.[0]?.name || "Various Locations",
              applyUrl: item.refs?.landing_page || "https://www.themuse.com"
            };
          });
          // Always show featured jobs first, then live Muse API results
          renderJobs(combineJobsWithFeatured(mapped), append);
          console.log("[Knowverse Gateway] Loaded jobs directly from The Muse API.");
          updateLoadMoreButton("load-more-jobs-btn", mapped.length);
          return;
        }
      }
    } catch (e) {
      console.warn("[Knowverse Gateway] Direct Muse API fetch failed. Using local client fallbacks...", e);
    }

    // Stage 6: Client Fallbacks (Static database — always our curated jobs)
    if (!append) {
      renderJobs(localFallbackJobs, false);
      updateLoadMoreButton("load-more-jobs-btn", 0);
    }
  };


  // Debounce helper
  const debounce = (func, delay = 300) => {
    let timeoutId;
    return (...args) => {
      clearTimeout(timeoutId);
      timeoutId = setTimeout(() => {
        func.apply(null, args);
      }, delay);
    };
  };

  // Filter State
  let currentSearch = "";
  let currentCountry = "in";
  let currentCategory = "internship";

  let opportunitiesPage = 1;
  let jobsPage = 1;

  const updateLoadMoreButton = (btnId, itemCount) => {
    const btn = document.getElementById(btnId);
    if (!btn) return;
    
    btn.classList.remove("loading");
    const span = btn.querySelector("span");
    if (span) {
      span.textContent = btnId === "load-more-opportunities-btn" ? "Load More Internships" : "Load More Jobs";
    }

    if (itemCount === 0) {
      btn.style.display = "none";
    } else {
      btn.style.display = "inline-flex";
    }
  };

  const triggerSearch = () => {
    opportunitiesPage = 1;
    jobsPage = 1;

    const query = currentSearch.trim() || currentCategory;
    const opportunitiesContainer = document.getElementById("opportunities-container");
    const jobsTableBody = document.getElementById("jobs-container");
    
    if (opportunitiesContainer) {
      opportunitiesContainer.innerHTML = `
        <div class="skeleton-card"></div>
        <div class="skeleton-card"></div>
        <div class="skeleton-card"></div>
      `;
    }
    if (jobsTableBody) {
      jobsTableBody.innerHTML = `
        <tr class="skeleton-row">
          <td colspan="4" style="height: 100px;"></td>
        </tr>
      `;
    }

    const optBtn = document.getElementById("load-more-opportunities-btn");
    const jobBtn = document.getElementById("load-more-jobs-btn");
    if (optBtn) optBtn.style.display = "none";
    if (jobBtn) jobBtn.style.display = "none";

    loadOpportunities(query, currentCountry, 1, false);
    const jobsQuery = currentSearch.trim() || `${currentCategory} job`;
    loadJobs(jobsQuery, currentCountry, 1, false);
  };

  // Bind Listeners
  const searchInput = document.getElementById("search-input");
  if (searchInput) {
    searchInput.addEventListener("input", debounce((e) => {
      currentSearch = e.target.value;
      triggerSearch();
    }, 300));
  }

  const countrySelect = document.getElementById("country-select");
  if (countrySelect) {
    countrySelect.addEventListener("change", (e) => {
      currentCountry = e.target.value;
      triggerSearch();
    });
  }

  const categoryPills = document.getElementById("category-pills");
  if (categoryPills) {
    categoryPills.addEventListener("click", (e) => {
      const pill = e.target.closest(".pill-btn");
      if (!pill) return;
      
      categoryPills.querySelectorAll(".pill-btn").forEach(btn => btn.classList.remove("active"));
      pill.classList.add("active");
      
      currentCategory = pill.getAttribute("data-category");
      
      if (searchInput) {
        searchInput.value = "";
        currentSearch = "";
      }

      triggerSearch();
    });
  }

  // Load More Listeners
  const loadMoreOpportunitiesBtn = document.getElementById("load-more-opportunities-btn");
  if (loadMoreOpportunitiesBtn) {
    loadMoreOpportunitiesBtn.addEventListener("click", () => {
      opportunitiesPage++;
      loadMoreOpportunitiesBtn.classList.add("loading");
      const span = loadMoreOpportunitiesBtn.querySelector("span");
      if (span) span.textContent = "Loading...";
      
      const query = currentSearch.trim() || currentCategory;
      loadOpportunities(query, currentCountry, opportunitiesPage, true);
    });
  }

  const loadMoreJobsBtn = document.getElementById("load-more-jobs-btn");
  if (loadMoreJobsBtn) {
    loadMoreJobsBtn.addEventListener("click", () => {
      jobsPage++;
      loadMoreJobsBtn.classList.add("loading");
      const span = loadMoreJobsBtn.querySelector("span");
      if (span) span.textContent = "Loading...";
      
      const jobsQuery = currentSearch.trim() || `${currentCategory} job`;
      loadJobs(jobsQuery, currentCountry, jobsPage, true);
    });
  }

  // Initial load
  triggerSearch();
});
