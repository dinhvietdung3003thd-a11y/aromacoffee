// CHÚ Ý: Đạt hãy sửa "7160" thành số Port bạn thấy khi chạy Visual Studio nhé
// Link API đúng phải là /api/Auth, không phải /swagger/index.html
const BASE_URL = "https://localhost:7160/api/Auth";

const app = document.getElementById('app');
const loginForm = document.getElementById('loginForm');
const signupForm = document.getElementById('signupForm');
const toggleButton = document.getElementById('toggleButton');
const overlayText = document.getElementById('overlayText');
const formTitle = document.getElementById('formTitle');
const loginTypeInput = document.getElementById('loginType');
const signupRoleInput = document.getElementById('signupRole');
const errorBox = document.getElementById('errorMessage');
const btnText = document.getElementById('btnText');
const btnSignup = document.getElementById('btnSignup');
const roleEmployee = document.getElementById('roleEmployee');
const roleAdmin = document.getElementById('roleAdmin');

let isSignUp = false;

function setRole(role) {
    loginTypeInput.value = role;
    signupRoleInput.value = role;
    localStorage.setItem('selectedRole', role);

    if (role === 'staff') {
        roleEmployee.classList.add('active');
        roleAdmin.classList.remove('active');
    } else {
        roleAdmin.classList.add('active');
        roleEmployee.classList.remove('active');
    }
}

function setMode(signUp) {
    isSignUp = signUp;
    app.classList.toggle('sign-up', signUp);

    if (signUp) {
        formTitle.textContent = 'SIGN UP';
        overlayText.textContent = 'Already have an account?';
        toggleButton.textContent = 'SIGN IN';
        loginForm.classList.add('hidden');
        signupForm.classList.remove('hidden');
    } else {
        formTitle.textContent = 'SIGN IN';
        overlayText.textContent = "Don't have an account?";
        toggleButton.textContent = 'SIGN UP';
        signupForm.classList.add('hidden');
        loginForm.classList.remove('hidden');
    }

    // Reset form fields + error
    loginForm.reset();
    signupForm.reset();
    errorBox.classList.add('hidden');
}

roleEmployee.addEventListener('click', () => setRole('staff'));
roleAdmin.addEventListener('click', () => setRole('admin'));

if (toggleButton) {
    toggleButton.addEventListener('click', () => setMode(!isSignUp));
}

const savedRole = localStorage.getItem('selectedRole') || 'staff';
setRole(savedRole);
setMode(false);

// --- Xử lý Gửi Form Đăng nhập ---
loginForm.onsubmit = async (e) => {
    e.preventDefault();

    // Reset trạng thái
    errorBox.classList.add('hidden');
    btnText.innerText = "Đang xử lý...";

    // 1. Dùng cùng endpoint login cho cả Employee và Admin
    const endpoint = '/login';

    // 2. Chuẩn bị dữ liệu khớp với LoginRequest.cs
    const loginData = {
        Username: document.getElementById('username').value.trim(),
        Password: document.getElementById('password').value
    };

    try {
        const response = await fetch(BASE_URL + endpoint, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(loginData)
        });

        // 3. Xử lý phản hồi từ Server
        if (response.ok) {
            const result = await response.json();

            // Lưu Token và thông tin vào LocalStorage
            localStorage.setItem('token', result.token);
            localStorage.setItem('user', JSON.stringify(result));

            const role = result.user?.Role || result.User?.Role || 'Staff';
            alert(`Đăng nhập thành công! Chào ${result.user?.FullName || result.User?.FullName}`);

            if (role === 'Admin') {
                window.location.href = "admin_dashboard.html";
            } else {
                window.location.href = "dashboard.html";
            }
        } else {
            // Lấy thông báo lỗi từ Backend
            const errorData = await response.json();
            errorBox.innerText = errorData.message || "Tài khoản hoặc mật khẩu không chính xác!";
            errorBox.classList.remove('hidden');
        }
    } catch (err) {
        console.error("Fetch Error:", err);
        errorBox.innerText = "Lỗi: Không thể kết nối tới Server. Đạt hãy kiểm tra lại Port!";
        errorBox.classList.remove('hidden');
    } finally {
        btnText.innerText = "SIGN IN";
    }
};

// --- Sign up handler (replace with real API if available) ---
signupForm.onsubmit = async (e) => {
    e.preventDefault();
    errorBox.classList.add('hidden');
    btnSignup.disabled = true;
    btnSignup.innerText = 'Đang xử lý...';

    const signupFullName = document.getElementById('signupFullName').value.trim();
    const signupPhone = document.getElementById('signupPhone').value.trim();
    const signupEmail = document.getElementById('signupEmail').value.trim();
    const signupUsername = document.getElementById('signupUsername').value.trim();
    const signupPassword = document.getElementById('signupPassword').value;
    const role = signupRoleInput.value;

    const endpoint = role === 'customer' ? '/customer/register' : '/register';
    const payload = role === 'customer'
        ? {
            Username: signupUsername,
            Password: signupPassword,
            FullName: signupFullName,
            PhoneNumber: signupPhone,
            Email: signupEmail
        }
        : {
            Username: signupUsername,
            Password: signupPassword,
            FullName: signupFullName,
            Role: role,
            PhoneNumber: signupPhone
        };

    try {
        const response = await fetch(BASE_URL + endpoint, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            const result = await response.json();
            alert(result.message || 'Đăng ký thành công!');
            setMode(false);
        } else {
            const errorData = await response.json();
            errorBox.innerText = errorData.message || 'Đăng ký thất bại. Vui lòng thử lại.';
            errorBox.classList.remove('hidden');
        }
    } catch (err) {
        console.error('Signup Error:', err);
        errorBox.innerText = 'Lỗi kết nối đến server. Vui lòng kiểm tra lại.';
        errorBox.classList.remove('hidden');
    } finally {
        btnSignup.disabled = false;
        btnSignup.innerText = 'SIGN UP';
    }
};
