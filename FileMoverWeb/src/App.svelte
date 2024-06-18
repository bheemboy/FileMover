<script>
  import { FileMoverService } from "./lib/FileMoverService";
  import Login from "./lib/Login.svelte";
  import Navbar from "./lib/Navbar.svelte";
  import FolderList from "./lib/FolderList.svelte";
  import CommandBar from "./lib/CommandBar.svelte";

  let BackendService = new FileMoverService("http://localhost:5000");
  let PathSettings = [];
  let SelectedTab = -1;

  let command = "";
  let busy = false;
  let percentage = 0;
  let folder = "";

  BackendService.on("Message", onMessage);

  function onMessage(message) {
    // console.log(message);
    if (message.Type == 1) {
      busy = message.Busy;
    } else if (message.Type == 2) {
      command = message.Command;
    } else if (message.Type == 3) {
      percentage = message.Percentage;
      folder = message.Folder;
    }
  }

</script>

<main>
  <Navbar
    bind:BackendService
    bind:PathSettings
    bind:SelectedTab
  />

  <FolderList
    bind:BackendService
    bind:PathSettings
    bind:SelectedTab
    bind:busy
  />

  <CommandBar bind:busy bind:command bind:percentage />

  <Login bind:BackendService />

</main>
