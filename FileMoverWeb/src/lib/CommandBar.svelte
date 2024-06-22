<script>
    export let command = "";
    export let busy = false;
    export let percentage = 0;
</script>

<div class="Command-Container">
    <p class="Command">
            {command}{(!busy && command.length>0) ? '... Done!' : ''}
    </p>
    <img
        class="busy-indicator"
        src="/images/busy.gif"
        alt="serverbusy"
        style="display: {busy ? 'block' : 'none'};"
    />
</div>
<div class="progressbar" style="display: {busy ? 'block' : 'none'};">
    <span class="progress" style="width: {percentage}%;"></span>
</div>

<style>
    .Command-Container {
        display: flex;
        margin-left: 25px;
        margin-right: 25px;
        background-color: #282828;
        align-items: center;
        box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .Command {
        margin-left: 15px;
        color: #326666
    }

    .busy-indicator {
        margin-left: 15px;
        width: 25px;
        height: 25px;
    }

    .progressbar {
        position: relative;
        /* max-width: 500px; */
        /* width: 100%; */
        margin-left: 25px;
        margin-right: 25px;
        /* margin: 0px auto 0; */
        height: 5px;
        background: #282828;
        overflow: hidden;
    }

    span.progress {
        position: absolute;
        left: 0;
        top: 0;
        bottom: 0;
        width: 45%;
        background: #326666;
        transition: all 0.3s;
    }
</style>
