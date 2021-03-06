// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
// The EventBusTest.java is a part of organization moe-org, under MIT License.
// See https://opensource.org/licenses/MIT for license information.
// Copyright (c) 2021-2022 moe-org All rights reserved.
// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

package moe.kawayi.org.utopia.core.test.event;

import moe.kawayi.org.utopia.core.event.EventBus;
import moe.kawayi.org.utopia.core.event.EventImpl;

import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

public class EventBusTest {

    private boolean called = false;

    private final EventBus<EventImpl<Boolean>> eventBus = new EventBus<>();

    @Test
    public void eventBusTest() {
        // test
        var id = eventBus.register(event -> {
            called = (boolean) event.getParameter().orElseThrow();
        });

        eventBus.fireEvent(new EventImpl<>(true, true));

        eventBus.unregister(id);

        eventBus.fireEvent(new EventImpl<>(false, true));

        Assertions.assertTrue(called);
    }

    @Test
    public void nullTest() {
        var bus = new EventBus<EventImpl<Integer>>();

        Assertions.assertThrows(NullPointerException.class, () -> {
            bus.register(null);
        });
        Assertions.assertThrows(NullPointerException.class, () -> {
            bus.unregister(null);
        });
        Assertions.assertThrows(NullPointerException.class, () -> {
            bus.fireEvent(null);
        });
    }
}
