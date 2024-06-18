<script>
  let password_value = "";
  let error_msg = "";
  let display_login = false;

  export let BackendService;
  BackendService.on("Initialize", onInitialized);
  BackendService.on("Logout", onLogout);
  
  // BackendService.logout();
  BackendService.initializeConnection();

  function onInitialized() {
    display_login = !BackendService.isLoggedIn();
  }

  function onLogout() {
    password_value = "";
    error_msg = "";
    display_login = !BackendService.isLoggedIn();
  }

  function handleKeyDown(event) {
    if (event.key === "Enter") {
      handleButtonClick();
    }
  }

  async function handleButtonClick() {
    try {
      if (!(await BackendService.loginWithPassword(password_value))) {
        error_msg = "Error while logging in. Try again.";
      } else {
        error_msg = "";
      }
    } catch (error) {
      console.log(error);      
    }
  }
</script>

<div class="fullscreen" style="display: {display_login ? 'flex' : 'none'};">
  <div class="login-dlg">
    <input
      type="password"
      id="password"
      placeholder="Enter password"
      bind:value={password_value}
      on:keydown={handleKeyDown}
    />
    <button id="login_btn" on:click={handleButtonClick}>Login</button>
    <div id="error-message">{error_msg}</div>
  </div>
</div>

<style>
  .fullscreen {
    display: none; /* Hidden by default */
    flex-wrap: wrap;
    align-content: center;
    overflow: scroll;
    justify-content: center;
    align-content: center;
    position: fixed; /* Stay in place */
    z-index: 500; /* Sit on top */
    left: 0;
    top: 0;
    width: 100%; /* Full width */
    height: 100%; /* Full height */
    overflow: auto; /* Enable scroll if needed */
    background-color: rgba(0, 0, 0, 0.5); /* Black trnsparent */
  }

  .login-dlg {
    background-color: #282828;
    max-width: 300px;
    max-height: 60px;
    padding: 30px;
    border-radius: 10px;
    border: 1px solid #353535;
    box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
    text-align: center;
  }

  input[type="password"] {
    padding: 10px;
    background-color: lightgray;
    border: 1px solid #ccc;
    border-radius: 5px;
    margin-bottom: 10px;
    width: 200px;
  }

  button {
    padding: 10px 20px;
    background-color: #4caf50;
    color: #fff;
    border: none;
    border-radius: 5px;
    cursor: pointer;
  }

  #error-message {
    color: #ce9178;
    font-size: 14px;
    margin-bottom: 10px;
    text-align: left;
    padding-left: 13px;
  }
</style>
